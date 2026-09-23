namespace NacionalSeguros.Contracts.Requests;

public record ScoringCallbackRequestDto(
    int PostulanteId,
    int VacanteId,
    int ScoreGeneral,
    int ScoreTecnico,
    int ScoreExperiencia,
    int ScoreCultural,
    string ExplicabilidadDetalle);

public record EvaluarMatchingRequestDto(
    int PostulanteId,
    int VacanteId);
