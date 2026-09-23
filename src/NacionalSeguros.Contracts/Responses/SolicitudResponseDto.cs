using System;
using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Responses;

public record SolicitudResponseDto(
    int SolicitudId,
    string Cargo,
    string Area,
    int SolicitanteId,
    int? DecisorId,
    string Seniority,
    string Prioridad,
    string Funciones,
    string EstadoNombre,
    DateTime CreatedDate,
    CatalogValueDto? Regional = null,
    CatalogValueDto? TipoSolicitud = null,
    CatalogValueDto? ModalidadTrabajo = null,
    string? SolicitanteNombre = null,
    string? SolicitanteEmail = null,
    string? SolicitanteCargo = null,
    string? DecisorNombre = null,
    string? EstadoCodigo = null,
    string? CanalOrigen = null,
    string? Codigo = null,
    string? Motivo = null,
    int? CantidadVacantes = null,
    string? Observaciones = null,
    
    // Perfil Requerido
    string? ObjetivoCargo = null,
    string? FormacionAcademica = null,
    string? ExperienciaMinima = null,
    string? ExperienciaIndispensable = null,
    string? ConocimientosTecnicos = null,
    string? HerramientasSistemas = null,
    string? CompetenciasClave = null,
    string? DisponibilidadRequerida = null,
    string? CriteriosExcluyentes = null,
    string? CriteriosDeseables = null,

    decimal? CompletitudPorcentaje = null,
    int? CamposDetectados = null,
    int? CamposEsperados = null,
    string? ProfileSummary = null,
    string? CaptureState = null,
    string? PdfDocumentUrl = null,
    string? JustificacionRechazo = null,
    DateTime? UltimaInteraccionN8N = null,
    string? TelefonoSolicitante = null,
    string? Comentario = null,
    string? ModifiedBy = null,
    DateTime? LastModifiedDate = null,
    IEnumerable<ObservacionResponseDto>? ObservacionesRrhh = null,
    IEnumerable<SolicitudComentarioResponseDto>? Comentarios = null,
    UltimaObservacionRrhhDto? UltimaObservacionRRHH = null,
    IReadOnlyList<string>? CamposRequeridosKeys = null
);
