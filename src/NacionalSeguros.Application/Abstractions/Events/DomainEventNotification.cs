using MediatR;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Application.Abstractions.Events;

public class DomainEventNotification<TDomainEvent> : INotification 
    where TDomainEvent : IDomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public TDomainEvent DomainEvent { get; }
}
