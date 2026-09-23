using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Events;

public record SolicitudActualizadaEvent(int SolicitudId, string EstadoAnteriorCodigo, string Changer, string Canal = "BackOffice") : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
