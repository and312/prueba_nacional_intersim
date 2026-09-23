using System;

namespace NacionalSeguros.Domain.Entities;

public class SolicitudResumen
{
    // Requerido por EF Core
    protected SolicitudResumen()
    {
    }

    public SolicitudResumen(
        int solicitudId,
        string? profileSummary,
        decimal? completitudPorcentaje,
        int? camposDetectados,
        int? camposEsperados,
        string captureState,
        decimal? captureConfidence,
        string recommendation,
        string? camposFaltantesJson,
        string? inconsistenciasJson,
        string agentName,
        string agentVersion,
        Guid? correlationId)
    {
        SolicitudId = solicitudId;
        ProfileSummary = profileSummary;
        CompletitudPorcentaje = completitudPorcentaje;
        CamposDetectados = camposDetectados;
        CamposEsperados = camposEsperados;
        CaptureState = captureState ?? throw new ArgumentNullException(nameof(captureState));
        CaptureConfidence = captureConfidence;
        Recommendation = recommendation ?? throw new ArgumentNullException(nameof(recommendation));
        CamposFaltantesJson = camposFaltantesJson;
        InconsistenciasJson = inconsistenciasJson;
        AgentName = agentName ?? throw new ArgumentNullException(nameof(agentName));
        AgentVersion = agentVersion ?? throw new ArgumentNullException(nameof(agentVersion));
        CorrelationId = correlationId;
        CreatedDate = DateTime.UtcNow;
    }

    public int ResumenId { get; private set; }
    public int SolicitudId { get; private set; }
    public string? ProfileSummary { get; private set; }
    public decimal? CompletitudPorcentaje { get; private set; }
    public int? CamposDetectados { get; private set; }
    public int? CamposEsperados { get; private set; }
    public string CaptureState { get; private set; } = string.Empty;
    public decimal? CaptureConfidence { get; private set; }
    public string Recommendation { get; private set; } = string.Empty;
    public string? CamposFaltantesJson { get; private set; }
    public string? InconsistenciasJson { get; private set; }
    public string AgentName { get; private set; } = string.Empty;
    public string AgentVersion { get; private set; } = string.Empty;
    public Guid? CorrelationId { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }

    // Propiedad de navegación
    public Solicitud Solicitud { get; private set; } = null!;

    public void Actualizar(
        string? profileSummary,
        decimal? completitudPorcentaje,
        int? camposDetectados,
        int? camposEsperados,
        string captureState,
        decimal? captureConfidence,
        string recommendation,
        string? camposFaltantesJson,
        string? inconsistenciasJson,
        string agentName,
        string agentVersion,
        Guid? correlationId)
    {
        ProfileSummary = profileSummary;
        CompletitudPorcentaje = completitudPorcentaje;
        CamposDetectados = camposDetectados;
        CamposEsperados = camposEsperados;
        CaptureState = captureState ?? throw new ArgumentNullException(nameof(captureState));
        CaptureConfidence = captureConfidence;
        Recommendation = recommendation ?? throw new ArgumentNullException(nameof(recommendation));
        CamposFaltantesJson = camposFaltantesJson;
        InconsistenciasJson = inconsistenciasJson;
        AgentName = agentName ?? throw new ArgumentNullException(nameof(agentName));
        AgentVersion = agentVersion ?? throw new ArgumentNullException(nameof(agentVersion));
        CorrelationId = correlationId;
        ModifiedDate = DateTime.UtcNow;
    }
}
