using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class AuditLog : Entity<long>
{
    protected AuditLog()
    {
    }

    public AuditLog(
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
        FechaHoraUTC = DateTime.UtcNow;
        UsuarioId = usuarioId;
        UsuarioNombre = usuarioNombre ?? throw new ArgumentNullException(nameof(usuarioNombre));
        Rol = rol ?? throw new ArgumentNullException(nameof(rol));
        Modulo = modulo ?? throw new ArgumentNullException(nameof(modulo));
        Entidad = entidad ?? throw new ArgumentNullException(nameof(entidad));
        EntidadId = entidadId;
        Accion = accion ?? throw new ArgumentNullException(nameof(accion));
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        Canal = canal ?? throw new ArgumentNullException(nameof(canal));
        CorrelationId = correlationId;
    }

    public DateTime FechaHoraUTC { get; private set; }
    public int? UsuarioId { get; private set; }
    public string UsuarioNombre { get; private set; } = string.Empty;
    public string Rol { get; private set; } = string.Empty;
    public string Modulo { get; private set; } = string.Empty;
    public string Entidad { get; private set; } = string.Empty;
    public int EntidadId { get; private set; }
    public string Accion { get; private set; } = string.Empty;
    public string? EstadoAnterior { get; private set; }
    public string? EstadoNuevo { get; private set; }
    public string Canal { get; private set; } = string.Empty;
    public Guid? CorrelationId { get; private set; }
}
