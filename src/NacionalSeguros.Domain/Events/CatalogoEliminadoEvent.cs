using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Events;

public record CatalogoEliminadoEvent(int CatalogoId, string Codigo) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
