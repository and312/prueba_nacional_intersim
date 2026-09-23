using System;

namespace NacionalSeguros.Contracts.Responses;

public record PerfilResponseDto(
    int PerfilId,
    int SolicitudId,
    string CargoNombre,
    string ProfesiogramaJson,
    int VersionActiva,
    string EstadoNombre,
    string EstadoCodigo,
    string Area,
    string Solicitante,
    DateTime FechaGeneracion,
    DateTime? UltimaModificacion,
    string SolicitudCodigo,
    string? PdfUrl = null,
    bool Activo = true
);
