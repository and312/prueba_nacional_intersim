using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Events;

public record SolicitudEstadoTransitadoEvent(int SolicitudId, int EstadoAnteriorId, int EstadoNuevoId, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
