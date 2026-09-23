using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class PerfilObservacion : Entity<int>
{
    protected PerfilObservacion()
    {
    }

    public PerfilObservacion(
        int perfilCargoId,
        int tipoObservacionId,
        string comentario,
        int usuarioSolicitanteId,
        int numeroIteracion)
    {
        PerfilCargoId = perfilCargoId;
        TipoObservacionId = tipoObservacionId;
        Comentario = comentario ?? throw new ArgumentNullException(nameof(comentario));
        UsuarioSolicitanteId = usuarioSolicitanteId;
        NumeroIteracion = numeroIteracion;
        EstadoObservacion = "Pendiente";
        CreatedDate = DateTime.UtcNow;
    }

    public int PerfilCargoId { get; private set; }
    public int TipoObservacionId { get; private set; }
    public string Comentario { get; private set; } = string.Empty;
    public int UsuarioSolicitanteId { get; private set; }
    public int NumeroIteracion { get; private set; }
    public string EstadoObservacion { get; private set; } = "Pendiente";
    public DateTime CreatedDate { get; private set; }
    public int? AtendidaPorUsuarioId { get; private set; }
    public DateTime? FechaAtencion { get; private set; }

    // Propiedades de navegación
    public PerfilCargo PerfilCargo { get; private set; } = null!;
    public TipoObservacion TipoObservacion { get; private set; } = null!;
    public Usuario UsuarioSolicitante { get; private set; } = null!;
    public Usuario? AtendidaPorUsuario { get; private set; }

    public void Atender(int atendidaPorUsuarioId)
    {
        EstadoObservacion = "Atendida";
        AtendidaPorUsuarioId = atendidaPorUsuarioId;
        FechaAtencion = DateTime.UtcNow;
    }

    public void Reabrir()
    {
        EstadoObservacion = "Pendiente";
        AtendidaPorUsuarioId = null;
        FechaAtencion = null;
    }

    public void SetEstadoObservacion(string estado)
    {
        EstadoObservacion = estado ?? throw new ArgumentNullException(nameof(estado));
    }
}
