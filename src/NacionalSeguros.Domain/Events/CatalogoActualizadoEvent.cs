using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Events;

public record CatalogoActualizadoEvent(int CatalogoId, string Codigo, string Nombre) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
