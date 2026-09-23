using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class StateHistory : Entity<long>
{
    // Requerido por EF Core
    protected StateHistory()
    {
    }

    public StateHistory(
        string entidad,
        int entidadId,
        int? estadoAnteriorId,
        int? estadoNuevoId,
        int? usuarioId,
        string? comentario,
        Guid? correlationId,
        int? iteracion = null,
        string? rol = null)
    {
        Entidad = entidad ?? throw new ArgumentNullException(nameof(entidad));
        EntidadId = entidadId;
        EstadoAnteriorId = estadoAnteriorId;
        EstadoNuevoId = estadoNuevoId;
        UsuarioId = usuarioId;
        Comentario = comentario;
        CorrelationId = correlationId;
        Fecha = DateTime.UtcNow;
        Iteracion = iteracion;
        Rol = rol;
    }

    public string Entidad { get; private set; } = string.Empty;
    public int EntidadId { get; private set; }
    public int? EstadoAnteriorId { get; private set; }
    public int? EstadoNuevoId { get; private set; }
    public int? UsuarioId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string? Comentario { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public int? Iteracion { get; private set; }
    public string? Rol { get; private set; }

    // Propiedades de navegación
    public Estado? EstadoAnterior { get; private set; }
    public Estado? EstadoNuevo { get; private set; }
    public Usuario? Usuario { get; private set; }
}
