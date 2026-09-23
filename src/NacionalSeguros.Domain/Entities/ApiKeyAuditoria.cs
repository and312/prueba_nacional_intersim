using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class ApiKeyAuditoria : Entity<long>
{
    protected ApiKeyAuditoria()
    {
    }

    public ApiKeyAuditoria(
        DateTime fechaHora,
        int? integracionId,
        string? integracionNombre,
        string? workflow,
        long? apiKeyId,
        string? apiKeyNombre,
        string endpoint,
        string metodo,
        string? ip,
        string? correlationId,
        int tiempoRespuestaMs,
        string resultado)
    {
        FechaHora = fechaHora;
        IntegracionId = integracionId;
        IntegracionNombre = integracionNombre;
        Workflow = workflow;
        ApiKeyId = apiKeyId;
        ApiKeyNombre = apiKeyNombre;
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        Metodo = metodo ?? throw new ArgumentNullException(nameof(metodo));
        IP = ip;
        CorrelationId = correlationId;
        TiempoRespuestaMs = tiempoRespuestaMs;
        Resultado = resultado ?? throw new ArgumentNullException(nameof(resultado));
    }

    public DateTime FechaHora { get; private set; }
    public int? IntegracionId { get; private set; }
    public string? IntegracionNombre { get; private set; }
    public string? Workflow { get; private set; }
    public long? ApiKeyId { get; private set; }
    public string? ApiKeyNombre { get; private set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string Metodo { get; private set; } = string.Empty;
    public string? IP { get; private set; }
    public string? CorrelationId { get; private set; }
    public int TiempoRespuestaMs { get; private set; }
    public string Resultado { get; private set; } = string.Empty;
}
