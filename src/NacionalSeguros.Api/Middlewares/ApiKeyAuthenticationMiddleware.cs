using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using System.Text.Json;

namespace NacionalSeguros.Api.Middlewares;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IApiKeyRepository apiKeyRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IApiKeyAuditoriaRepository auditoriaRepository)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Solo interceptar peticiones destinadas a /api/internal/
        if (!path.StartsWith("/api/internal", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        string headerName = "X-Internal-Api-Key";
        string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        
        Guid? correlationId = null;
        if (context.Items.TryGetValue("X-Correlation-ID", out var cid) && cid != null && Guid.TryParse(cid.ToString(), out var parsedId))
        {
            correlationId = parsedId;
        }

        if (!context.Request.Headers.TryGetValue(headerName, out var extractedApiKey) || string.IsNullOrWhiteSpace(extractedApiKey))
        {
            headerName = "Internal-Api-Key";
            context.Request.Headers.TryGetValue(headerName, out extractedApiKey);
        }

        if (string.IsNullOrWhiteSpace(extractedApiKey))
        {
            _logger.LogWarning("Falta la cabecera X-Internal-Api-Key en la solicitud.");
            
            // Registrar auditoría de falla
            var auditRecord = new ApiKeyAuditoria(
                fechaHora: DateTime.UtcNow,
                integracionId: null,
                integracionNombre: null,
                workflow: null,
                apiKeyId: null,
                apiKeyNombre: null,
                endpoint: path,
                metodo: context.Request.Method,
                ip: ipAddress,
                correlationId: correlationId?.ToString(),
                tiempoRespuestaMs: 0,
                resultado: "Falla (401 - Cabecera Faltante)"
            );
            await auditoriaRepository.AddAsync(auditRecord);
            await unitOfWork.SaveChangesAsync();

            await WriteUnauthorizedResponseAsync(context, "Falta la cabecera X-Internal-Api-Key.");
            return;
        }

        string rawKey = extractedApiKey.ToString().Trim();
        string keyHash = ComputeSha256Hash(rawKey);

        var apiKey = await apiKeyRepository.GetByHashAsync(keyHash);

        if (apiKey == null)
        {
            _logger.LogWarning("Intento de acceso con API Key inexistente. IP: {IP}, Hash: {Hash}", ipAddress, keyHash);

            var auditRecord = new ApiKeyAuditoria(
                fechaHora: DateTime.UtcNow,
                integracionId: null,
                integracionNombre: null,
                workflow: null,
                apiKeyId: null,
                apiKeyNombre: null,
                endpoint: path,
                metodo: context.Request.Method,
                ip: ipAddress,
                correlationId: correlationId?.ToString(),
                tiempoRespuestaMs: 0,
                resultado: "Falla (401 - Hash Inexistente)"
            );
            await auditoriaRepository.AddAsync(auditRecord);
            await unitOfWork.SaveChangesAsync();

            await WriteUnauthorizedResponseAsync(context, "API Key inválida.");
            return;
        }

        // Validar si la integración dueña está activa (si está asignada)
        if (apiKey.Integracion != null && apiKey.Integracion.Estado != "Activo")
        {
            _logger.LogWarning("API Key pertenece a una integración inactiva. Integracion: {Integracion}, Key: {KeyId}", apiKey.Integracion.Nombre, apiKey.Id);

            var auditRecord = new ApiKeyAuditoria(
                fechaHora: DateTime.UtcNow,
                integracionId: apiKey.IntegracionId,
                integracionNombre: apiKey.Integracion.Nombre,
                workflow: apiKey.Workflow,
                apiKeyId: apiKey.Id,
                apiKeyNombre: apiKey.Nombre,
                endpoint: path,
                metodo: context.Request.Method,
                ip: ipAddress,
                correlationId: correlationId?.ToString(),
                tiempoRespuestaMs: 0,
                resultado: "Falla (403 - Integracion Inactiva)"
            );
            await auditoriaRepository.AddAsync(auditRecord);
            await unitOfWork.SaveChangesAsync();

            await WriteForbiddenResponseAsync(context, "La integración asociada está deshabilitada.");
            return;
        }

        // Validar estado y expiración de la API Key
        if (apiKey.Estado != "Activo" || (apiKey.FechaExpiracion.HasValue && apiKey.FechaExpiracion.Value < DateTime.UtcNow))
        {
            string motivo = apiKey.Estado != "Activo" ? $"Estado: {apiKey.Estado}" : "Expirada";
            _logger.LogWarning("Intento de acceso con API Key inactiva o expirada. Motivo: {Motivo}, Key: {KeyId}", motivo, apiKey.Id);

            var auditRecord = new ApiKeyAuditoria(
                fechaHora: DateTime.UtcNow,
                integracionId: apiKey.IntegracionId,
                integracionNombre: apiKey.Integracion?.Nombre,
                workflow: apiKey.Workflow,
                apiKeyId: apiKey.Id,
                apiKeyNombre: apiKey.Nombre,
                endpoint: path,
                metodo: context.Request.Method,
                ip: ipAddress,
                correlationId: correlationId?.ToString(),
                tiempoRespuestaMs: 0,
                resultado: $"Falla (403 - {motivo})"
            );
            await auditoriaRepository.AddAsync(auditRecord);
            await unitOfWork.SaveChangesAsync();

            await WriteForbiddenResponseAsync(context, $"La API Key no está activa o ha expirado. ({motivo})");
            return;
        }

        // Registrar último uso y dirección IP
        apiKey.RegistrarUso(ipAddress);
        apiKeyRepository.Update(apiKey);
        await unitOfWork.SaveChangesAsync();

        string email = !string.IsNullOrEmpty(apiKey.CreadoPor) && apiKey.CreadoPor.Contains("@")
            ? apiKey.CreadoPor
            : "admin@nacionalseguros.com.bo";

        // Mapear claims de identidad y permisos
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, apiKey.Nombre),
            new Claim("workflow", apiKey.Workflow),
            new Claim("ApiKeyId", apiKey.Id.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

        if (apiKey.IntegracionId.HasValue)
        {
            claims.Add(new Claim("IntegracionId", apiKey.IntegracionId.Value.ToString()));
        }

        // Cargar permisos desde tabla de unión o fallback a columna de texto
        if (apiKey.ApiKeyPermisos != null && apiKey.ApiKeyPermisos.Any())
        {
            claims.AddRange(apiKey.ApiKeyPermisos.Select(ap => new Claim("permission", ap.Permiso.Codigo)));
        }
        else
        {
            claims.AddRange(
                apiKey.Permisos
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => new Claim("permission", p.Trim()))
            );
        }

        var identity = new ClaimsIdentity(claims, "ApiKey");
        context.User = new ClaimsPrincipal(identity);

        // Guardar ApiKeyId y Workflow en HttpContext para trazabilidad
        context.Items["ApiKeyId"] = apiKey.Id;
        context.Items["WorkflowOrigen"] = apiKey.Workflow;
        if (apiKey.IntegracionId.HasValue)
        {
            context.Items["IntegracionId"] = apiKey.IntegracionId.Value;
        }

        // Ejecutar llamada midiendo tiempo de respuesta
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        string outcome = "Exito";

        try
        {
            await _next(context);
            outcome = $"Exito ({context.Response.StatusCode})";
        }
        catch (Exception ex)
        {
            outcome = $"Falla (Error 500: {ex.Message})";
            throw;
        }
        finally
        {
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;

            // Registrar auditoría de éxito o error
            var auditRecord = new ApiKeyAuditoria(
                fechaHora: DateTime.UtcNow,
                integracionId: apiKey.IntegracionId,
                integracionNombre: apiKey.Integracion?.Nombre,
                workflow: apiKey.Workflow,
                apiKeyId: apiKey.Id,
                apiKeyNombre: apiKey.Nombre,
                endpoint: path,
                metodo: context.Request.Method,
                ip: ipAddress,
                correlationId: correlationId?.ToString(),
                tiempoRespuestaMs: (int)elapsedMs,
                resultado: outcome.Length > 50 ? outcome.Substring(0, 50) : outcome
            );

            await auditoriaRepository.AddAsync(auditRecord);
            await unitOfWork.SaveChangesAsync();
        }
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

    private static async Task WriteUnauthorizedResponseAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString();

        var errorResponse = new ApiErrorDto
        {
            Code = "UNAUTHORIZED_API_KEY",
            Message = message,
            Detail = "El acceso a las APIs internas requiere una cabecera X-Internal-Api-Key válida.",
            CorrelationId = correlationId
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    }

    private static async Task WriteForbiddenResponseAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        var correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString();

        var errorResponse = new ApiErrorDto
        {
            Code = "FORBIDDEN_API_KEY",
            Message = message,
            Detail = "La API Key no tiene permisos suficientes o se encuentra inactiva.",
            CorrelationId = correlationId
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    }
}
