using System;

namespace NacionalSeguros.Contracts.Requests;

public record SolicitudUpdateDto(
    string Cargo,
    int? RegionalId,
    int? CantidadVacantes,
    int? TipoSolicitudId,
    string? Motivo,
    string? ObjetivoCargo,
    string? FormacionAcademica,
    string? ExperienciaMinima,
    string? ExperienciaIndispensable,
    string? ConocimientosTecnicos,
    string? HerramientasSistemas,
    string? CompetenciasClave,
    string? CriteriosExcluyentes,
    string? CriteriosDeseables,
    string Funciones,
    int? ModalidadTrabajoId,
    string? DisponibilidadRequerida,
    string Seniority,
    string Prioridad,
    string? Observaciones = null
);
