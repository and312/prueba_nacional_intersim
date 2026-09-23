using System;

namespace NacionalSeguros.Contracts.Responses;

public record StateHistoryResponseDto(
    long HistoryId,
    int EntidadId,
    string EstadoAnterior,
    string EstadoNuevo,
    string CambiadoPor,
    DateTime FechaCambio,
    string Justificacion,
    string Rol,
    string? TipoComentario = null,
    string? EstadoRelacionado = null,
    int? Iteracion = null,
    Guid? CorrelationId = null
);
