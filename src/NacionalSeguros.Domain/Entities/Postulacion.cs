using System;
using NacionalSeguros.Domain.Events;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Postulacion : Entity<int>
{
    // Requerido por EF Core
    protected Postulacion()
    {
    }

    public Postulacion(
        int postulanteId,
        int vacanteId,
        int estadoPipelineId,
        decimal pretensionSalarial,
        string createdBy)
    {
        if (postulanteId < 0) throw new ArgumentException("El ID del postulante no puede ser negativo.", nameof(postulanteId));
        if (vacanteId <= 0) throw new ArgumentException("El ID de la vacante debe ser mayor a 0.", nameof(vacanteId));
        if (estadoPipelineId <= 0) throw new ArgumentException("El ID del estado del pipeline debe ser mayor a 0.", nameof(estadoPipelineId));
        if (pretensionSalarial < 0) throw new ArgumentException("La pretensión salarial no puede ser negativa.", nameof(pretensionSalarial));

        PostulanteId = postulanteId;
        VacanteId = vacanteId;
        EstadoPipelineId = estadoPipelineId;
        PretensionSalarial = pretensionSalarial;
        FechaPostulacion = DateTime.UtcNow;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;

        RaiseDomainEvent(new PostulanteRegistradoEvent(PostulanteId, VacanteId, createdBy));
    }

    public int PostulanteId { get; private set; }
    public int VacanteId { get; private set; }
    public DateTime FechaPostulacion { get; private set; }
    public int EstadoPipelineId { get; private set; }
    public decimal PretensionSalarial { get; private set; }

    // Campos de Auditoría
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Propiedades de navegación
    public Postulante Postulante { get; private set; } = null!;
    public Vacante Vacante { get; private set; } = null!;
    public Estado EstadoPipeline { get; private set; } = null!;

    // Métodos de Dominio
    public void Transitar(Estado nuevoEstado, string modificadoPor)
    {
        if (nuevoEstado == null) throw new ArgumentNullException(nameof(nuevoEstado));
        if (!string.Equals(nuevoEstado.Entidad, "Postulante", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El estado especificado no pertenece al pipeline de Postulantes.");
        }

        int estadoAnteriorId = EstadoPipelineId;
        EstadoPipelineId = nuevoEstado.Id;
        EstadoPipeline = nuevoEstado;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new PostulanteEstadoTransitadoEvent(PostulanteId, VacanteId, estadoAnteriorId, EstadoPipelineId, modificadoPor));
    }

    public void Eliminar()
    {
        IsDeleted = true;
    }
}
