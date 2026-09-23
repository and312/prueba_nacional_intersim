using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Sesion : Entity<long>
{
    protected Sesion()
    {
    }

    public Sesion(int usuarioId, string refreshToken, DateTime fechaExpiracion)
    {
        UsuarioId = usuarioId;
        RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
        FechaExpiracion = fechaExpiracion;
        Activa = true;
        CreatedDate = DateTime.UtcNow;
    }

    public int UsuarioId { get; private set; }
    public string RefreshToken { get; private set; } = string.Empty;
    public DateTime FechaExpiracion { get; private set; }
    public bool Activa { get; private set; }
    public DateTime CreatedDate { get; private set; }

    // Propiedad de navegación (opcional)
    public Usuario? Usuario { get; private set; }

    public void Desactivar()
    {
        Activa = false;
    }

    public bool EstaExpirada()
    {
        return DateTime.UtcNow >= FechaExpiracion;
    }
}
