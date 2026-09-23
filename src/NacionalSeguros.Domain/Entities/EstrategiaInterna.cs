using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class EstrategiaInterna : Entity<int>
{
    // Requerido por EF Core
    protected EstrategiaInterna()
    {
    }

    public EstrategiaInterna(int matchingEjecucionId)
    {
        if (matchingEjecucionId <= 0) throw new ArgumentException("MatchingEjecucionId debe ser mayor a 0", nameof(matchingEjecucionId));

        MatchingEjecucionId = matchingEjecucionId;
        FechaCreacion = DateTime.UtcNow;
    }

    public int MatchingEjecucionId { get; private set; }
    public string? Prioridad { get; private set; }
    public string? Justificacion { get; private set; }
    public string? PlanAccion { get; private set; }
    public string? PlanEvaluacion { get; private set; }
    public string? MensajeContingencia { get; private set; }
    public string? Conclusion { get; private set; }
    public int? EstadoId { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaModificacion { get; private set; }

    // Propiedades de navegación
    public MatchingEjecucion MatchingEjecucion { get; private set; } = null!;
    public Estado? Estado { get; private set; }

    public void Actualizar(
        string? prioridad,
        string? justificacion,
        string? planAccion,
        string? planEvaluacion,
        string? mensajeContingencia,
        string? conclusion,
        int? estadoId)
    {
        Prioridad = prioridad;
        Justificacion = justificacion;
        PlanAccion = planAccion;
        PlanEvaluacion = planEvaluacion;
        MensajeContingencia = mensajeContingencia;
        Conclusion = conclusion;
        EstadoId = estadoId;
        FechaModificacion = DateTime.UtcNow;
    }
}
