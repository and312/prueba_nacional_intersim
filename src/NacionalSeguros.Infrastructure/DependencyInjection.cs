using Microsoft.Extensions.DependencyInjection;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Application.Abstractions.Notifications;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Infrastructure.Security;
using NacionalSeguros.Infrastructure.Notifications;
using NacionalSeguros.Infrastructure.Audit;
using NacionalSeguros.Infrastructure.Cache;

namespace NacionalSeguros.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Registrar Servicios de Criptografía y Tokens
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IMfaService, MfaService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();

        // Registrar Servicios de Notificaciones y Auditoría Ledger
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuditService, AuditService>();

        // Registrar Cache del Módulo de Catálogos Maestros
        services.AddMemoryCache();
        services.AddScoped<ICatalogoCacheService, CatalogoCacheService>();

        return services;
    }
}
