using System;

namespace NacionalSeguros.Contracts.Responses;

public record ObservacionResponseDto(
    long ObservacionId,
    DateTime Fecha,
    string Hora,
    int? UsuarioRrhhId,
    string UsuarioRrhhNombre,
    string Rol,
    string Comentario,
    string Observacion,
    string? CampoObservado,
    string EstadoAsociado,
    DateTime FechaCreacion,
    DateTime? FechaModificacion = null
);
