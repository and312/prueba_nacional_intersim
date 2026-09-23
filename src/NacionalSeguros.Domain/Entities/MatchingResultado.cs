using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class MatchingResultado : Entity<int>
{
    // Requerido por EF Core
    protected MatchingResultado()
    {
    }

    public MatchingResultado(
        int matchingEjecucionId,
        int postulanteId,
        string origen,
        decimal porcentajeMatching,
        string clasificacion,
        int posicionRanking,
        decimal puntajeObtenido,
        decimal puntajeMaximo,
        string fortalezasJson,
        string brechasJson,
        string desgloseCriteriosJson,
        string? tipoDescarte = null,
        string? motivoExclusion = null)
    {
        if (matchingEjecucionId <= 0) throw new ArgumentException("MatchingEjecucionId debe ser mayor a 0", nameof(matchingEjecucionId));
        if (postulanteId <= 0) throw new ArgumentException("PostulanteId debe ser mayor a 0", nameof(postulanteId));
        if (string.IsNullOrWhiteSpace(origen)) throw new ArgumentException("Origen no puede estar vacío", nameof(origen));
        if (string.IsNullOrWhiteSpace(clasificacion)) throw new ArgumentException("Clasificacion no puede estar vacía", nameof(clasificacion));

        MatchingEjecucionId = matchingEjecucionId;
        PostulanteId = postulanteId;
        Origen = origen;
        PorcentajeMatching = porcentajeMatching;
        Clasificacion = clasificacion;
        PosicionRanking = posicionRanking;
        PuntajeObtenido = puntajeObtenido;
        PuntajeMaximo = puntajeMaximo;
        FortalezasJson = fortalezasJson ?? "[]";
        BrechasJson = brechasJson ?? "[]";
        DesgloseCriteriosJson = desgloseCriteriosJson ?? "[]";
        TipoDescarte = tipoDescarte;
        MotivoExclusion = motivoExclusion;
        CreatedDate = DateTime.UtcNow;
    }

    public int MatchingEjecucionId { get; private set; }
    public int PostulanteId { get; private set; }
    public string Origen { get; private set; } = string.Empty;
    public decimal PorcentajeMatching { get; private set; }
    public string Clasificacion { get; private set; } = string.Empty;
    public int PosicionRanking { get; private set; }
    public decimal PuntajeObtenido { get; private set; }
    public decimal PuntajeMaximo { get; private set; }
    public string FortalezasJson { get; private set; } = "[]";
    public string BrechasJson { get; private set; } = "[]";
    public string DesgloseCriteriosJson { get; private set; } = "[]";
    public string? TipoDescarte { get; private set; }
    public string? MotivoExclusion { get; private set; }
    public DateTime CreatedDate { get; private set; }

    // Propiedades de navegación
    public MatchingEjecucion MatchingEjecucion { get; private set; } = null!;

    public void Actualizar(
        decimal porcentajeMatching,
        string clasificacion,
        int posicionRanking,
        decimal puntajeObtenido,
        decimal puntajeMaximo,
        string fortalezasJson,
        string brechasJson,
        string desgloseCriteriosJson,
        string? tipoDescarte = null,
        string? motivoExclusion = null)
    {
        if (string.IsNullOrWhiteSpace(clasificacion)) throw new ArgumentException("Clasificacion no puede estar vacía", nameof(clasificacion));

        PorcentajeMatching = porcentajeMatching;
        Clasificacion = clasificacion;
        PosicionRanking = posicionRanking;
        PuntajeObtenido = puntajeObtenido;
        PuntajeMaximo = puntajeMaximo;
        FortalezasJson = fortalezasJson ?? "[]";
        BrechasJson = brechasJson ?? "[]";
        DesgloseCriteriosJson = desgloseCriteriosJson ?? "[]";
        TipoDescarte = tipoDescarte;
        MotivoExclusion = motivoExclusion;
    }
}
