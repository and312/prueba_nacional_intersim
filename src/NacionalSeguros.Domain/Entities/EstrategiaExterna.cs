using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class EstrategiaExterna : Entity<int>
{
    // Requerido por EF Core
    protected EstrategiaExterna()
    {
    }

    public EstrategiaExterna(int matchingEjecucionId)
    {
        if (matchingEjecucionId <= 0) throw new ArgumentException("MatchingEjecucionId debe ser mayor a 0", nameof(matchingEjecucionId));

        MatchingEjecucionId = matchingEjecucionId;
        FechaCreacion = DateTime.UtcNow;
    }

    public int MatchingEjecucionId { get; private set; }
    public string? Prioridad { get; private set; }
    public string? Justificacion { get; private set; }
    public string? CriteriosDificiles { get; private set; }
    public string? PublicoObjetivo { get; private set; }
    public string? CanalesSugeridos { get; private set; }
    public string? PlanAccion { get; private set; }
    public string? BriefingEditable { get; private set; }
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
        string? criteriosDificiles,
        string? publicoObjetivo,
        string? canalesSugeridos,
        string? planAccion,
        string? briefingEditable,
        string? conclusion,
        int? estadoId)
    {
        Prioridad = prioridad;
        Justificacion = justificacion;
        CriteriosDificiles = criteriosDificiles;
        PublicoObjetivo = publicoObjetivo;
        CanalesSugeridos = canalesSugeridos;
        PlanAccion = planAccion;
        BriefingEditable = briefingEditable;
        Conclusion = conclusion;
        EstadoId = estadoId;
        FechaModificacion = DateTime.UtcNow;
    }
}
