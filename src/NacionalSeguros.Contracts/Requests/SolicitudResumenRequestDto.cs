using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NacionalSeguros.Contracts.Requests;

public record InconsistenciaDto(
    string Campo,
    string Detalle
);

public record SolicitudResumenRequestDto(
    string? ProfileSummary,
    string CaptureState,
    decimal? CaptureConfidence,
    string Recommendation,
    List<string>? MissingFields,
    List<InconsistenciaDto>? Inconsistencias,
    [property: JsonPropertyName("agenteName")] string AgentName,
    [property: JsonPropertyName("agenteVersion")] string AgentVersion,
    Guid? CorrelationId,
    int? CamposEsperados = null,
    int? CamposDetectados = null,
    int? CamposFaltantes = null,
    decimal? CompletitudPorcentaje = null
);
