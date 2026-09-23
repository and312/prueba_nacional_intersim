using System;
using System.Threading.Tasks;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;

namespace NacionalSeguros.Infrastructure.Audit;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task LogActionAsync(
        int? usuarioId,
        string usuarioNombre,
        string rol,
        string modulo,
        string entidad,
        int entidadId,
        string accion,
        string? estadoAnterior,
        string? estadoNuevo,
        string canal,
        Guid? correlationId)
    {
        // Sanitizar datos sensibles preventivamente (salariales, claves)
        string? sanitizedAnterior = SanitizarSensible(estadoAnterior);
        string? sanitizedNuevo = SanitizarSensible(estadoNuevo);

        var auditLog = new AuditLog(
            usuarioId,
            usuarioNombre,
            rol,
            modulo,
            entidad,
            entidadId,
            accion,
            sanitizedAnterior,
            sanitizedNuevo,
            canal,
            correlationId);

        await _auditLogRepository.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string? SanitizarSensible(string? payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return payload;
        }

        // Términos sensibles que deben ser protegidos en auditoría
        string[] palabrasClave = { "clave", "password", "secreto", "mfa", "salario", "salarial", "token" };

        foreach (var palabra in palabrasClave)
        {
            if (payload.Contains(palabra, StringComparison.OrdinalIgnoreCase))
            {
                return "[DATOS ENMASCARADOS POR SEGURIDAD]";
            }
        }

        return payload;
    }
}
