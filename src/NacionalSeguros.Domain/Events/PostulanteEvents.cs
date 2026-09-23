using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Events;

public record PostulanteRegistradoEvent(int PostulanteId, int VacanteId, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public record PostulanteEstadoTransitadoEvent(int PostulanteId, int VacanteId, int EstadoAnteriorId, int EstadoNuevoId, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
