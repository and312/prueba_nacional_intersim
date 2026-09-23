using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class WSSession : Entity<Guid>
{
    private WSSession() : base(Guid.NewGuid())
    {
    }

    public WSSession(
        string channelIdentifier,
        string normalizedIdentifier,
        string activeAgent,
        string sessionStatus,
        string? temporaryDataJson,
        string? pendingFieldsJson,
        string? perfilSessionDataJson = null) : base(Guid.NewGuid())
    {
        ChannelIdentifier = channelIdentifier ?? throw new ArgumentNullException(nameof(channelIdentifier));
        NormalizedIdentifier = normalizedIdentifier ?? throw new ArgumentNullException(nameof(normalizedIdentifier));
        ActiveAgent = activeAgent ?? throw new ArgumentNullException(nameof(activeAgent));
        SessionStatus = sessionStatus ?? throw new ArgumentNullException(nameof(sessionStatus));
        TemporaryDataJson = temporaryDataJson;
        PendingFieldsJson = pendingFieldsJson;
        PerfilSessionDataJson = perfilSessionDataJson;
        CreatedAt = DateTime.UtcNow;
        LastInteractionAt = DateTime.UtcNow;
    }

    public string ChannelIdentifier { get; private set; } = string.Empty;
    public string NormalizedIdentifier { get; private set; } = string.Empty;
    public string ActiveAgent { get; private set; } = string.Empty;
    public string SessionStatus { get; private set; } = string.Empty;
    public string? TemporaryDataJson { get; private set; }
    public string? PendingFieldsJson { get; private set; }
    public string? PerfilSessionDataJson { get; private set; }
    public DateTime LastInteractionAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Actualizar(
        string channelIdentifier,
        string activeAgent,
        string sessionStatus,
        string? temporaryDataJson,
        string? pendingFieldsJson,
        string? perfilSessionDataJson = null)
    {
        ChannelIdentifier = channelIdentifier ?? throw new ArgumentNullException(nameof(channelIdentifier));
        ActiveAgent = activeAgent ?? throw new ArgumentNullException(nameof(activeAgent));
        SessionStatus = sessionStatus ?? throw new ArgumentNullException(nameof(sessionStatus));
        TemporaryDataJson = temporaryDataJson;
        PendingFieldsJson = pendingFieldsJson;
        PerfilSessionDataJson = perfilSessionDataJson;
        LastInteractionAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarParcial(
        string? activeAgent,
        string? sessionStatus,
        string? temporaryDataJson,
        string? pendingFieldsJson,
        string? perfilSessionDataJson = null)
    {
        if (activeAgent != null) ActiveAgent = activeAgent;
        if (sessionStatus != null) SessionStatus = sessionStatus;
        if (temporaryDataJson != null) TemporaryDataJson = temporaryDataJson;
        if (pendingFieldsJson != null) PendingFieldsJson = pendingFieldsJson;
        if (perfilSessionDataJson != null) PerfilSessionDataJson = perfilSessionDataJson;
        LastInteractionAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
