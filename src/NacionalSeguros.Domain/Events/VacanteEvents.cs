using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Events;

public record VacanteCreadaEvent(int VacanteId, int SolicitudId, int PerfilCargoId, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public record VacantePublicadaEvent(int VacanteId, int SolicitudId, int PerfilCargoId, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public record VacanteCerradaEvent(int VacanteId, int SolicitudId, int PerfilCargoId, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public record VacantePausadaEvent(int VacanteId, int SolicitudId, int PerfilCargoId, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public record VacanteReanudadaEvent(int VacanteId, int SolicitudId, int PerfilCargoId, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public record VacanteActualizadaEvent(int VacanteId, decimal BandaSalarialMin, decimal BandaSalarialMax, string Changer) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
