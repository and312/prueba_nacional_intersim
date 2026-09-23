using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class HistorialApiKey : Entity<long>
{
    protected HistorialApiKey()
    {
    }

    public HistorialApiKey(
        long apiKeyId,
        string accion,
        string realizadoPor,
        string? detalle)
    {
        ApiKeyId = apiKeyId;
        Accion = accion ?? throw new ArgumentNullException(nameof(accion));
        Fecha = DateTime.UtcNow;
        RealizadoPor = realizadoPor ?? throw new ArgumentNullException(nameof(realizadoPor));
        Detalle = detalle;
    }

    public long ApiKeyId { get; private set; }
    public string Accion { get; private set; } = string.Empty; // "Creado", "Rotado", "Revocado", etc.
    public DateTime Fecha { get; private set; }
    public string RealizadoPor { get; private set; } = string.Empty;
    public string? Detalle { get; private set; }

    // Navigation
    public ApiKey ApiKey { get; private set; } = null!;
}
