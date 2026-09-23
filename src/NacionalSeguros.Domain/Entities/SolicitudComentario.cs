using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class SolicitudComentario : Entity<int>
{
    // Required by EF Core
    protected SolicitudComentario()
    {
    }

    public SolicitudComentario(int solicitudId, int usuarioId, string texto, string? estadoAsociado = null, int? iteracion = null, string? tipoComentario = null)
    {
        SolicitudId = solicitudId;
        UsuarioId = usuarioId;
        Texto = texto ?? throw new ArgumentNullException(nameof(texto));
        Fecha = DateTime.UtcNow;
        EstadoAsociado = estadoAsociado;
        Iteracion = iteracion;
        TipoComentario = tipoComentario;
    }

    public int SolicitudId { get; private set; }
    public int UsuarioId { get; private set; }
    public string Texto { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string? EstadoAsociado { get; private set; }
    public int? Iteracion { get; private set; }
    public string? TipoComentario { get; private set; }

    // Navigation properties
    public Solicitud Solicitud { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;
}
