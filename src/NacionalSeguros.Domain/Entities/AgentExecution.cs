using System;

namespace NacionalSeguros.Domain.Entities;

public class AgentExecution
{
    // Requerido por EF Core
    protected AgentExecution()
    {
    }

    public AgentExecution(
        int agenteId,
        int promptVersionId,
        int usuarioId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int duracionMs,
        string inputJson,
        string outputJson,
        string resultadoStatus,
        int tokensInput,
        int tokensOutput,
        decimal costoEstimado,
        Guid correlationId)
    {
        AgenteId = agenteId;
        PromptVersionId = promptVersionId;
        UsuarioId = usuarioId;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        DuracionMs = duracionMs;
        InputJson = inputJson ?? throw new ArgumentNullException(nameof(inputJson));
        OutputJson = outputJson ?? throw new ArgumentNullException(nameof(outputJson));
        ResultadoStatus = resultadoStatus ?? throw new ArgumentNullException(nameof(resultadoStatus));
        TokensInput = tokensInput;
        TokensOutput = tokensOutput;
        CostoEstimado = costoEstimado;
        CorrelationId = correlationId;
    }

    public long ExecutionId { get; private set; }
    public int AgenteId { get; private set; }
    public int PromptVersionId { get; private set; }
    public int UsuarioId { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime FechaFin { get; private set; }
    public int DuracionMs { get; private set; }
    public string InputJson { get; private set; } = string.Empty;
    public string OutputJson { get; private set; } = string.Empty;
    public string ResultadoStatus { get; private set; } = "Success";
    public int TokensInput { get; private set; }
    public int TokensOutput { get; private set; }
    public decimal CostoEstimado { get; private set; }
    public Guid CorrelationId { get; private set; }
}
