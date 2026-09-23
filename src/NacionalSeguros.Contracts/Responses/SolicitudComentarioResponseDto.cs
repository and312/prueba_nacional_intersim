using System;

namespace NacionalSeguros.Contracts.Responses;

public record SolicitudComentarioResponseDto(
    int ComentarioId,
    int SolicitudId,
    string UsuarioNombre,
    string UsuarioCorreo,
    string Texto,
    DateTime Fecha,
    string Hora,
    string? TipoComentario = null,
    string? EstadoRelacionado = null,
    int? Iteracion = null
);
