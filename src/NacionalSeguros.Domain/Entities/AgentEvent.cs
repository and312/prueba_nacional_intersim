using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class AgentEvent : Entity<int>
{
    private AgentEvent() : base(0)
    {
    }

    public AgentEvent(
        string channelType,
        string channelIdentifier,
        string eventType,
        string? description,
        string eventSource,
        Guid? correlationId,
        string? metadataJson,
        string? relatedEntityType,
        string? relatedEntityId) : base(0)
    {
        ChannelType = channelType ?? throw new ArgumentNullException(nameof(channelType));
        ChannelIdentifier = channelIdentifier ?? throw new ArgumentNullException(nameof(channelIdentifier));
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Description = description;
        EventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
        CorrelationId = correlationId;
        MetadataJson = metadataJson;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        CreatedAt = DateTime.UtcNow;
    }

    public string ChannelType { get; private set; } = string.Empty;
    public string ChannelIdentifier { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string EventSource { get; private set; } = string.Empty;
    public Guid? CorrelationId { get; private set; }
    public string? MetadataJson { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
