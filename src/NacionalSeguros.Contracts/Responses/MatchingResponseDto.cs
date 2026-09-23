using System;

namespace NacionalSeguros.Contracts.Responses;

public record MatchingResultDto(
    int PostulanteId,
    int VacanteId,
    decimal ScoreCoincidencia,
    string CoincidenciasText,
    string BrechasText,
    DateTime FechaEvaluacion);

public record ScoringResultDto(
    int PostulanteId,
    int VacanteId,
    int ScoreSkills,
    int ScoreExperiencia,
    decimal ScoreFinal,
    string JustificacionText,
    DateTime FechaEvaluacion);

public record MatchingExpedienteDto(
    MatchingResultDto? Matching,
    ScoringResultDto? Scoring);
