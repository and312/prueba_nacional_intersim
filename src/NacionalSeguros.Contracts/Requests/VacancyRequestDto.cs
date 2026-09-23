using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NacionalSeguros.Contracts.Requests;

public class VacancyRequestCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string RequestedByUsuarioId { get; set; } = string.Empty;
    public string RequestedByUserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? RegionalId { get; set; }
    public int? TipoSolicitudId { get; set; }
    public int? WorkModeId { get; set; }
    public int VacancyCount { get; set; }
    public string Seniority { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? CargoId { get; set; }
    
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

    public string Channel { get; set; } = string.Empty;
    public string? WorkflowOrigen { get; set; }
    public string? Comments { get; set; }
}

public class VacancyRequestUpdateDto
{
    public string? Title { get; set; }
    public string? RequestedByUsuarioId { get; set; }
    public string? RequestedByUserId { get; set; }
    public string? Reason { get; set; }
    public int? RegionalId { get; set; }
    public int? TipoSolicitudId { get; set; }
    public int? WorkModeId { get; set; }
    public int? VacancyCount { get; set; }
    public string? Seniority { get; set; }
    public string? Priority { get; set; }
    
    // Perfil
    public string? ObjetivoCargo { get; set; }
    public string? ExperienciaMinima { get; set; }
    public string? ConocimientosTecnicos { get; set; }
    public string? MainFunctions { get; set; }
    
    // Recomendados
    public string? FormacionAcademica { get; set; }
    public string? ExperienciaIndispensable { get; set; }
    public string? HerramientasSistemas { get; set; }
    public string? CompetenciasClave { get; set; }
    public string? DisponibilidadRequerida { get; set; }
    public string? CriteriosExcluyentes { get; set; }
    public string? CriteriosDeseables { get; set; }

    public string? Channel { get; set; }
    public string? WorkflowOrigen { get; set; }
    public string? Comments { get; set; }
}

public class ObservationResponseDto
{
    public string RespondedByUsuarioId { get; set; } = string.Empty;
    public string RespondedByUserId { get; set; } = string.Empty;
    public string ResponseText { get; set; } = string.Empty;
}

public class AgentEventCreateDto
{
    public string ChannelType { get; set; } = string.Empty;
    public string ChannelIdentifier { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EventSource { get; set; } = string.Empty;
    public Guid? CorrelationId { get; set; }
    public object? Metadata { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
}

public class AgentEventResponseDto
{
    public int Id { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string ChannelIdentifier { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EventSource { get; set; } = string.Empty;
    public Guid? CorrelationId { get; set; }
    public object? Metadata { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WSSessionCreateUpdateDto
{
    public string ChannelIdentifier { get; set; } = string.Empty;
    public string NormalizedIdentifier { get; set; } = string.Empty;
    public string ActiveAgent { get; set; } = string.Empty;
    public string SessionStatus { get; set; } = string.Empty;
    public object? TemporaryData { get; set; }
    public List<string>? PendingFields { get; set; }
    public object? PerfilSessionData { get; set; }
}

public class WSSessionPatchDto
{
    public string? ActiveAgent { get; set; }
    public string? SessionStatus { get; set; }
    public object? TemporaryData { get; set; }
    public List<string>? PendingFields { get; set; }
    public object? PerfilSessionData { get; set; }
}

public class WSSessionResponseDto
{
    public Guid Id { get; set; }
    public string ChannelIdentifier { get; set; } = string.Empty;
    public string NormalizedIdentifier { get; set; } = string.Empty;
    public string ActiveAgent { get; set; } = string.Empty;
    public string SessionStatus { get; set; } = string.Empty;
    public object? TemporaryData { get; set; }
    public List<string>? PendingFields { get; set; }
    public object? PerfilSessionData { get; set; }
    public DateTime LastInteractionAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EtapaUsuarioDto? EtapaUsuario { get; set; }
}

public class EtapaUsuarioDto
{
    public DateTime CalculadoEn { get; set; } = DateTime.UtcNow;
    public EtapaUsuarioResumenDto Resumen { get; set; } = new();
    public List<int> IndiceSolicitudesConPerfil { get; set; } = new();
    public List<PerfilDestacadoDto> PerfilesDestacados { get; set; } = new();
}

public class EtapaUsuarioResumenDto
{
    public int SolicitudesEnCurso { get; set; }
    public int SolicitudesAprobadasSinPerfil { get; set; }
    public int SolicitudesConPerfil { get; set; }
    public int PerfilesEsperandoAlUsuario { get; set; }
}

public class PerfilDestacadoDto
{
    public int SolicitudId { get; set; }
    public string SolicitudCodigo { get; set; } = string.Empty;
    public int PerfilId { get; set; }
    public string PerfilCodigo { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string EstadoCodigo { get; set; } = string.Empty;
    public string EstadoEtiqueta { get; set; } = string.Empty;
    public bool EsperaAlUsuario { get; set; }
    public DateTime FechaModificacion { get; set; }
}
