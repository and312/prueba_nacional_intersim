using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NacionalSeguros.Application.Abstractions.Audit;

namespace NacionalSeguros.Api.Middlewares;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        var method = context.Request.Method;

        // Auditar solo mutaciones de datos exitosas
        if (method == HttpMethods.Post || method == HttpMethods.Put || method == HttpMethods.Delete)
        {
            string path = context.Request.Path;
            
            // Habilitar buffering para leer el body
            context.Request.EnableBuffering();

            string bodyContent = string.Empty;
            using (var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true))
            {
                bodyContent = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Resetear posición
            }

            await _next(context);

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                try
                {
                    var userClaims = context.User;
                    int? userId = null;
                    string userName = "Anónimo";
                    string rol = "Ninguno";

                    var subClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier);
                    if (subClaim != null && int.TryParse(subClaim.Value, out int id))
                    {
                        userId = id;
                    }

                    var nameClaim = userClaims.FindFirst(ClaimTypes.Name);
                    if (nameClaim != null)
                    {
                        userName = nameClaim.Value;
                    }

                    var roleClaim = userClaims.FindFirst(ClaimTypes.Role);
                    if (roleClaim != null)
                    {
                        rol = roleClaim.Value;
                    }

                    Guid? correlationId = null;
                    if (context.Items.TryGetValue("X-Correlation-ID", out var cid) && cid != null && Guid.TryParse(cid.ToString(), out var parsedId))
                    {
                        correlationId = parsedId;
                    }

                    string modulo = DeterminarModulo(path);
                    string entidad = DeterminarEntidad(path);
                    int entidadId = 0;

                    // Si hay un ID en el path, intentar extraerlo como EntidadId
                    string[] pathParts = path?.Split('/') ?? Array.Empty<string>();
                    foreach (var part in pathParts)
                    {
                        if (int.TryParse(part, out int val) && val > 0)
                        {
                            entidadId = val;
                            break;
                        }
                    }

                    string canal = "API";
                    if (path != null && path.StartsWith("/api/internal", StringComparison.OrdinalIgnoreCase))
                    {
                        canal = "N8N";
                    }

                    await auditService.LogActionAsync(
                        usuarioId: userId,
                        usuarioNombre: userName,
                        rol: rol,
                        modulo: modulo,
                        entidad: entidad,
                        entidadId: entidadId,
                        accion: ($"{method} {path}").Length > 50 ? ($"{method} {path}").Substring(0, 50) : $"{method} {path}",
                        estadoAnterior: null,
                        estadoNuevo: bodyContent,
                        canal: canal,
                        correlationId: correlationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar logs de auditoría en el Middleware.");
                }
            }
        }
        else
        {
            await _next(context);
        }
    }

    private static string DeterminarModulo(string path)
    {
        if (path.Contains("/auth", StringComparison.OrdinalIgnoreCase)) return "Seguridad";
        if (path.Contains("/usuarios", StringComparison.OrdinalIgnoreCase)) return "Seguridad";
        if (path.Contains("/solicitudes", StringComparison.OrdinalIgnoreCase)) return "Solicitudes";
        if (path.Contains("/perfiles", StringComparison.OrdinalIgnoreCase)) return "Perfiles";
        if (path.Contains("/vacantes", StringComparison.OrdinalIgnoreCase)) return "Vacantes";
        if (path.Contains("/postulantes", StringComparison.OrdinalIgnoreCase)) return "Postulantes";
        return "General";
    }

    private static string DeterminarEntidad(string path)
    {
        if (path.Contains("/usuarios", StringComparison.OrdinalIgnoreCase)) return "Usuarios";
        if (path.Contains("/auth/login", StringComparison.OrdinalIgnoreCase)) return "Sesiones";
        if (path.Contains("/solicitudes", StringComparison.OrdinalIgnoreCase)) return "Solicitudes";
        if (path.Contains("/perfiles", StringComparison.OrdinalIgnoreCase)) return "PerfilesCargo";
        if (path.Contains("/vacantes", StringComparison.OrdinalIgnoreCase)) return "Vacantes";
        if (path.Contains("/postulantes", StringComparison.OrdinalIgnoreCase)) return "Postulantes";
        return "General";
    }
}
