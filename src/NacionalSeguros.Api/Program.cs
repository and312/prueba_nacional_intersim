using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AutoMapper;
using NacionalSeguros.Api.Middlewares;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;
using NacionalSeguros.Persistence.Repositories;
using NacionalSeguros.Application;
using NacionalSeguros.Persistence;
using NacionalSeguros.Infrastructure;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Serilog con enriquecimiento y salida a consola
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Resolver JWT Secret de forma segura sin fallback en duro (CWE-798)
string GetJwtSecret(IConfiguration configuration)
{
    var secret = configuration["Jwt:Secret"];
    if (!string.IsNullOrEmpty(secret))
    {
        return secret;
    }

    // Fallback efímero para entorno de desarrollo local (CWE-798 compliance)
    Log.Warning("JWT Secret no configurado en variables de entorno o Secrets Manager. Generando clave efímera de instancia única. ¡No apto para escalado horizontal!");
    byte[] keyBytes = new byte[32];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(keyBytes);
    }
    string generated = Convert.ToBase64String(keyBytes);
    configuration["Jwt:Secret"] = generated;
    return generated;
}

string jwtSecret = GetJwtSecret(builder.Configuration);

// 3. Autenticación y Autorización JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "NacionalSeguros.SIR",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "NacionalSeguros.SIR.Clients",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. Configurar OpenTelemetry (Tracing)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("NacionalSeguros.Api"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

// 5. Configurar Redis para Caché Distribuido e Idempotencia
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "SIR_";
});

// 6. Configurar Entity Framework Core 9 (SQL Server 2022)
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var sessionInterceptor = sp.GetRequiredService<NacionalSeguros.Persistence.Interceptors.SetSessionContextInterceptor>();
    var eventInterceptor = sp.GetRequiredService<NacionalSeguros.Persistence.Interceptors.PublishDomainEventsInterceptor>();
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => {
            sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            sqlOptions.MaxBatchSize(1); // Deshabilita batching (MERGE) para compatibilidad con tablas Ledger
        })
    .AddInterceptors(sessionInterceptor, eventInterceptor);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 7. Registro de dependencias de Arquitectura Limpia (Módulo 01)
builder.Services.AddApplication();
builder.Services.AddPersistence();
builder.Services.AddInfrastructure();

// 8. Configurar MediatR y AutoMapper
var applicationAssembly = AppDomain.CurrentDomain.Load("NacionalSeguros.Application");
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
builder.Services.AddAutoMapper(applicationAssembly);

// 9. Versionado de la API
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
})
.AddMvc();

// 9.5. Configurar CORS para desarrollo local con el Frontend Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// 10. Configuración de Controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 11. Configurar Swagger con inyección de JWT y API Keys
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SIR Nacional Seguros API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Encabezado de autorización JWT usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Encabezado de autenticación con API Key interna para n8n. Ejemplo: \"X-Internal-Api-Key: {mi_clave}\"",
        Name = "X-Internal-Api-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 12. Health Checks (Base de datos y Redis)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRedis(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379", name: "redis");

// 11.5. Configurar Webhooks globalmente en la capa Shared
NacionalSeguros.Shared.Primitives.WebhookSettings.AresResumidorUrl = builder.Configuration["Webhooks:AresResumidorUrl"] ?? "https://nacional-seguros-dev.isia.cloud/webhook/ares-resumidor";
NacionalSeguros.Shared.Primitives.WebhookSettings.EnviarAreaWebhookUrl = builder.Configuration["Webhooks:EnviarAreaWebhookUrl"] ?? "https://nacional-seguros-dev.isia.cloud/webhook/enviar_area";
NacionalSeguros.Shared.Primitives.WebhookSettings.PerfilVacanteResumenUrl = builder.Configuration["Webhooks:PerfilVacanteResumenUrl"] ?? "https://nacional-seguros-dev.isia.cloud/webhook/perfil_vacante_resumen";

var app = builder.Build();

// Configurar Middleware Pipeline (Clean Architecture / Security Guidelines)
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseMiddleware<AuditMiddleware>();

// Habilitar la política de CORS registrada
app.UseCors("AllowAngularDev");

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIR Nacional Seguros API v1");
    c.SwaggerEndpoint("v1/swagger.json", "SIR Nacional Seguros API v1");
    c.RoutePrefix = "swagger";
});

var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsDir))
{
    Directory.CreateDirectory(uploadsDir);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsDir),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

// 12.5 Sincronizar API Keys desde configuración a base de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var keysSection = configuration.GetSection("InternalApi:ApiKeys");
        if (keysSection.Exists())
        {
            // Asegurar existencia de la integración por defecto
            var defaultIntegracion = await dbContext.Integraciones.FirstOrDefaultAsync(i => i.Codigo == "SYS_INT");
            if (defaultIntegracion == null)
            {
                defaultIntegracion = new Integracion(
                    "Sistema de Integraciones Internas",
                    "SYS_INT",
                    "Integración del sistema por defecto para flujos n8n preexistentes",
                    "Workflow",
                    "Administrador",
                    "admin@nacionalseguros.com.bo",
                    "SystemSeed"
                );
                await dbContext.Integraciones.AddAsync(defaultIntegracion);
                await dbContext.SaveChangesAsync();
            }

            foreach (var keyConfig in keysSection.GetChildren())
            {
                var nombre = keyConfig["Nombre"];
                var key = keyConfig["Key"];
                var workflow = keyConfig["Workflow"];
                var descripcion = keyConfig["Descripcion"] ?? string.Empty;
                var estado = keyConfig["Estado"] ?? "Activo";
                var permisos = keyConfig["Permisos"] ?? string.Empty;

                if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(workflow))
                {
                    using var sha256 = SHA256.Create();
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key.Trim()));
                    var sb = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        sb.Append(bytes[i].ToString("x2"));
                    }
                    string hash = sb.ToString();

                    var exists = await dbContext.ApiKeys.AnyAsync(a => a.ApiKeyHash == hash);
                    if (!exists)
                    {
                        var newKey = new ApiKey(nombre, hash, workflow, descripcion, estado, null, "SystemSeed", permisos, defaultIntegracion.Id);
                        await dbContext.ApiKeys.AddAsync(newKey);
                    }
                }
            }
            await dbContext.SaveChangesAsync();
        }

        // Sembrado automático de Áreas de Cargo y Cargos iniciales
        await NacionalSeguros.Persistence.Seeders.AreaCargoSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al sincronizar API Keys durante la inicialización.");
    }
}

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();
