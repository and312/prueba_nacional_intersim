using System;

namespace NacionalSeguros.Contracts.Responses;

public class VacancyRequestResponseDto
{
    public int Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? CargoId { get; set; }
    public string RequestedByUsuarioId { get; set; } = string.Empty;
    public string RequestedByUserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int VacancyCount { get; set; }
    public string Seniority { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string DecisionMaker { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public CatalogValueDto? Regional { get; set; }
    public CatalogValueDto? TipoSolicitud { get; set; }
    public CatalogValueDto? ModalidadTrabajo { get; set; }

    // Perfil
    public string ObjetivoCargo { get; set; } = string.Empty;
    public string ExperienciaMinima { get; set; } = string.Empty;
    public string ConocimientosTecnicos { get; set; } = string.Empty;
    public string MainFunctions { get; set; } = string.Empty;

    // Recomendados
    public string? FormacionAcademica { get; set; }
    public string? ExperienciaIndispensable { get; set; }
    public string? HerramientasSistemas { get; set; }
    public string? CompetenciasClave { get; set; }
    public string? DisponibilidadRequerida { get; set; }
    public string? CriteriosExcluyentes { get; set; }
    public string? CriteriosDeseables { get; set; }
}
