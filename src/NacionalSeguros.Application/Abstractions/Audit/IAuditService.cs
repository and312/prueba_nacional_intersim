using System;
using System.Threading.Tasks;

namespace NacionalSeguros.Application.Abstractions.Audit;

public interface IAuditService
{
    Task LogActionAsync(
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
        Guid? correlationId);
}
