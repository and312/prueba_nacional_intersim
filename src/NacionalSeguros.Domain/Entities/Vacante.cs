using System;
using NacionalSeguros.Domain.Events;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Vacante : Entity<int>
{
    // Requerido por EF Core
    protected Vacante()
    {
    }

    public Vacante(
        int perfilCargoId,
        int solicitudId,
        int estadoId,
        decimal bandaSalarialMin,
        decimal bandaSalarialMax,
        string createdBy)
    {
        if (perfilCargoId <= 0) throw new ArgumentException("El ID del perfil de cargo debe ser mayor a 0.", nameof(perfilCargoId));
        if (solicitudId <= 0) throw new ArgumentException("El ID de la solicitud debe ser mayor a 0.", nameof(solicitudId));
        if (estadoId <= 0) throw new ArgumentException("El ID del estado debe ser mayor a 0.", nameof(estadoId));
        if (bandaSalarialMin < 0) throw new ArgumentException("La banda salarial mínima no puede ser negativa.", nameof(bandaSalarialMin));
        if (bandaSalarialMax < 0) throw new ArgumentException("La banda salarial máxima no puede ser negativa.", nameof(bandaSalarialMax));
        if (bandaSalarialMax < bandaSalarialMin) throw new ArgumentException("La banda salarial máxima no puede ser menor que la mínima.", nameof(bandaSalarialMax));

        PerfilCargoId = perfilCargoId;
        SolicitudId = solicitudId;
        EstadoId = estadoId;
        BandaSalarialMin = bandaSalarialMin;
        BandaSalarialMax = bandaSalarialMax;
        FechaApertura = DateTime.UtcNow;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;

        RaiseDomainEvent(new VacanteCreadaEvent(Id, SolicitudId, PerfilCargoId, createdBy));
    }

    public int PerfilCargoId { get; private set; }
    public int SolicitudId { get; private set; }
    public int EstadoId { get; private set; }
    public DateTime FechaApertura { get; private set; }
    public DateTime? FechaCierre { get; private set; }
    public decimal BandaSalarialMin { get; private set; }
    public decimal BandaSalarialMax { get; private set; }

    // Campos de Auditoría
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Propiedades de navegación
    public PerfilCargo PerfilCargo { get; private set; } = null!;
    public Solicitud Solicitud { get; private set; } = null!;
    public Estado Estado { get; private set; } = null!;

    // Métodos de Dominio
    public void Publicar(Estado estadoPublicada, string modificadoPor)
    {
        if (estadoPublicada == null) throw new ArgumentNullException(nameof(estadoPublicada));
        if (!string.Equals(estadoPublicada.Entidad, "Vacante", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El estado especificado no pertenece a la entidad Vacante.");
        }

        // Regla: No se puede publicar si el perfil no está aprobado
        if (PerfilCargo == null || PerfilCargo.EstadoId != 3) // 3 = SOL-APR (Aprobada) o PERF-04
        {
            // Nota: En la entidad, si PerfilCargo no está pre-cargado, asumimos que la validación se hace en el Handler,
            // pero si está presente, lo validamos aquí de manera defensiva.
            if (PerfilCargo != null)
            {
                throw new InvalidOperationException("No se puede publicar una vacante si el perfil de cargo no está aprobado.");
            }
        }

        EstadoId = estadoPublicada.Id;
        Estado = estadoPublicada;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new VacantePublicadaEvent(Id, SolicitudId, PerfilCargoId, modificadoPor));
    }

    public void Cerrar(Estado estadoContratada, string modificadoPor)
    {
        if (estadoContratada == null) throw new ArgumentNullException(nameof(estadoContratada));
        if (!string.Equals(estadoContratada.Entidad, "Vacante", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El estado especificado no pertenece a la entidad Vacante.");
        }

        EstadoId = estadoContratada.Id;
        Estado = estadoContratada;
        FechaCierre = DateTime.UtcNow;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new VacanteCerradaEvent(Id, SolicitudId, PerfilCargoId, modificadoPor));
    }

    public void Cancelar(Estado estadoCerrada, string modificadoPor)
    {
        if (estadoCerrada == null) throw new ArgumentNullException(nameof(estadoCerrada));
        if (!string.Equals(estadoCerrada.Entidad, "Vacante", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El estado especificado no pertenece a la entidad Vacante.");
        }

        EstadoId = estadoCerrada.Id;
        Estado = estadoCerrada;
        FechaCierre = DateTime.UtcNow;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new VacanteCerradaEvent(Id, SolicitudId, PerfilCargoId, modificadoPor));
    }

    public void Actualizar(decimal bandaSalarialMin, decimal bandaSalarialMax, string modificadoPor)
    {
        if (bandaSalarialMin < 0) throw new ArgumentException("La banda salarial mínima no puede ser negativa.", nameof(bandaSalarialMin));
        if (bandaSalarialMax < 0) throw new ArgumentException("La banda salarial máxima no puede ser negativa.", nameof(bandaSalarialMax));
        if (bandaSalarialMax < bandaSalarialMin) throw new ArgumentException("La banda salarial máxima no puede ser menor que la mínima.", nameof(bandaSalarialMax));

        BandaSalarialMin = bandaSalarialMin;
        BandaSalarialMax = bandaSalarialMax;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new VacanteActualizadaEvent(Id, BandaSalarialMin, BandaSalarialMax, modificadoPor));
    }

    public void Pausar(Estado estadoCerrada, string modificadoPor)
    {
        if (estadoCerrada == null) throw new ArgumentNullException(nameof(estadoCerrada));
        if (!string.Equals(estadoCerrada.Entidad, "Vacante", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El estado especificado no pertenece a la entidad Vacante.");
        }

        EstadoId = estadoCerrada.Id;
        Estado = estadoCerrada;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new VacantePausadaEvent(Id, SolicitudId, PerfilCargoId, modificadoPor));
    }

    public void Reanudar(Estado estadoReanudada, string modificadoPor)
    {
        if (estadoReanudada == null) throw new ArgumentNullException(nameof(estadoReanudada));
        if (!string.Equals(estadoReanudada.Entidad, "Vacante", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El estado especificado no pertenece a la entidad Vacante.");
        }

        EstadoId = estadoReanudada.Id;
        Estado = estadoReanudada;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new VacanteReanudadaEvent(Id, SolicitudId, PerfilCargoId, modificadoPor));
    }

    public void Eliminar()
    {
        IsDeleted = true;
    }
}
