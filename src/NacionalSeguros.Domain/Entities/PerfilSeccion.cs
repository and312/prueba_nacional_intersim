using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class PerfilSeccion : Entity<int>
{
    // Requerido por EF Core
    protected PerfilSeccion()
    {
    }

    public PerfilSeccion(
        int perfilCargoId,
        int numeroSeccion,
        string nombreSeccion,
        string contenido,
        int orden,
        string usuarioActualizacion)
    {
        PerfilCargoId = perfilCargoId;
        NumeroSeccion = numeroSeccion;
        NombreSeccion = nombreSeccion ?? throw new ArgumentNullException(nameof(nombreSeccion));
        Contenido = contenido ?? throw new ArgumentNullException(nameof(contenido));
        Orden = orden;
        UltimaActualizacion = DateTime.UtcNow;
        UsuarioActualizacion = usuarioActualizacion ?? throw new ArgumentNullException(nameof(usuarioActualizacion));
    }

    public int PerfilCargoId { get; private set; }
    public int NumeroSeccion { get; private set; }
    public string NombreSeccion { get; private set; } = string.Empty;
    public string Contenido { get; private set; } = string.Empty;
    public int Orden { get; private set; }
    public DateTime UltimaActualizacion { get; private set; }
    public string UsuarioActualizacion { get; private set; } = string.Empty;

    // Propiedad de navegación
    public PerfilCargo PerfilCargo { get; private set; } = null!;

    public void ActualizarContenido(string nuevoContenido, string usuario)
    {
        Contenido = nuevoContenido ?? throw new ArgumentNullException(nameof(nuevoContenido));
        UsuarioActualizacion = usuario ?? throw new ArgumentNullException(nameof(usuario));
        UltimaActualizacion = DateTime.UtcNow;
    }
}
