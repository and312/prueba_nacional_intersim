using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class PerfilAuditoria : Entity<int>
{
    // Requerido por EF Core
    protected PerfilAuditoria()
    {
    }

    public PerfilAuditoria(
        int perfilCargoId,
        int? perfilSeccionId,
        int version,
        string? seccionModificada,
        string? valorAnterior,
        string? valorNuevo,
        string usuario,
        string? motivoCambio = null,
        string? estadoPerfil = null)
    {
        PerfilCargoId = perfilCargoId;
        PerfilSeccionId = perfilSeccionId;
        Version = version;
        SeccionModificada = seccionModificada;
        ValorAnterior = valorAnterior;
        ValorNuevo = valorNuevo;
        Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
        FechaHora = DateTime.UtcNow;
        MotivoCambio = motivoCambio;
        EstadoPerfil = estadoPerfil;
    }

    public int PerfilCargoId { get; private set; }
    public int? PerfilSeccionId { get; private set; }
    public int Version { get; private set; }
    public string? SeccionModificada { get; private set; }
    public string? ValorAnterior { get; private set; }
    public string? ValorNuevo { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime FechaHora { get; private set; }
    public string? MotivoCambio { get; private set; }
    public string? EstadoPerfil { get; private set; }

    // Propiedades de navegación
    public PerfilCargo PerfilCargo { get; private set; } = null!;
    public PerfilSeccion? PerfilSeccion { get; private set; }
}
