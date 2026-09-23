using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Domain.Services;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Matchings.Commands.EjecutarMatching;

public class EjecutarMatchingCommandHandler : IRequestHandler<EjecutarMatchingCommand, Result<MatchingResultDto>>
{
    private readonly IMatchingRepository _matchingRepository;
    private readonly IPostulanteRepository _postulanteRepository;
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IIAProvider _iaProvider;
    private readonly IUnitOfWork _unitOfWork;

    public EjecutarMatchingCommandHandler(
        IMatchingRepository matchingRepository,
        IPostulanteRepository postulanteRepository,
        IVacanteRepository vacanteRepository,
        IIAProvider iaProvider,
        IUnitOfWork unitOfWork)
    {
        _matchingRepository = matchingRepository ?? throw new ArgumentNullException(nameof(matchingRepository));
        _postulanteRepository = postulanteRepository ?? throw new ArgumentNullException(nameof(postulanteRepository));
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _iaProvider = iaProvider ?? throw new ArgumentNullException(nameof(iaProvider));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<MatchingResultDto>> Handle(EjecutarMatchingCommand request, CancellationToken cancellationToken)
    {
        var postulante = await _postulanteRepository.GetByIdAsync(request.PostulanteId);
        if (postulante == null)
        {
            return Result.Failure<MatchingResultDto>(new Error("Postulante.NotFound", $"El postulante con ID {request.PostulanteId} no existe."));
        }

        var vacante = await _vacanteRepository.GetByIdAsync(request.VacanteId);
        if (vacante == null)
        {
            return Result.Failure<MatchingResultDto>(new Error("Vacante.NotFound", $"La vacante con ID {request.VacanteId} no existe."));
        }

        // Simulación de entrada para el prompt del Agente de Matching
        string systemPrompt = "Eres un Agente de Matching de IA experto en reclutamiento. Compara el perfil del cargo de la vacante con el CV del postulante y devuelve un JSON con: Score (decimal entre 0 y 100), Coincidencias (string) y Brechas (string).";
        string userPrompt = $"Vacante: {vacante.Id}. Postulante: {postulante.Nombres} {postulante.Apellidos}, Correo: {postulante.Correo}.";

        DateTime inicio = DateTime.UtcNow;

        // Llamar al proveedor de IA
        var inferenceResult = await _iaProvider.ProcessInferenceAsync(systemPrompt, userPrompt, cancellationToken);

        DateTime fin = DateTime.UtcNow;
        int duracionMs = (int)(fin - inicio).TotalMilliseconds;

        // Registrar la ejecución del agente
        var execution = new AgentExecution(
            agenteId: 4, // Agente de Matching
            promptVersionId: 1,
            usuarioId: 1, // Reclutador/System
            fechaInicio: inicio,
            fechaFin: fin,
            duracionMs: duracionMs,
            inputJson: JsonSerializer.Serialize(new { VacanteId = vacante.Id, PostulanteId = postulante.Id }),
            outputJson: inferenceResult.OutputText,
            resultadoStatus: "Success",
            tokensInput: inferenceResult.TokensInput,
            tokensOutput: inferenceResult.TokensOutput,
            costoEstimado: inferenceResult.CostoUSD,
            correlationId: Guid.NewGuid()
        );

        await _matchingRepository.AddAgentExecutionAsync(execution);
        // Guardar cambios para generar el ExecutionId en base de datos
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Parsear el resultado de la inferencia
        decimal score = 50.0m;
        string coincidencias = "Análisis completado.";
        string brechas = "Sin brechas críticas detectadas.";

        try
        {
            using var doc = JsonDocument.Parse(inferenceResult.OutputText);
            var root = doc.RootElement;
            if (root.TryGetProperty("Score", out var scoreProp))
            {
                score = scoreProp.GetDecimal();
            }
            if (root.TryGetProperty("Coincidencias", out var coincProp))
            {
                coincidencias = coincProp.GetString() ?? coincidencias;
            }
            if (root.TryGetProperty("Brechas", out var brechasProp))
            {
                brechas = brechasProp.GetString() ?? brechas;
            }
        }
        catch
        {
            // Fallback si no es JSON estructurado
            coincidencias = inferenceResult.OutputText;
        }

        var matching = new Matching(
            postulanteId: postulante.Id,
            vacanteId: vacante.Id,
            scoreCoincidencia: score,
            coincidenciasText: coincidencias,
            brechasText: brechas,
            executionId: execution.ExecutionId
        );

        await _matchingRepository.AddMatchingAsync(matching);

        _unitOfWork.TransitionComment = $"Ejecución de matching inteligente de IA para candidato {postulante.Id} y vacante {vacante.Id}.";
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new MatchingResultDto(
            PostulanteId: matching.PostulanteId,
            VacanteId: matching.VacanteId,
            ScoreCoincidencia: matching.ScoreCoincidencia,
            CoincidenciasText: matching.CoincidenciasText,
            BrechasText: matching.BrechasText,
            FechaEvaluacion: matching.CreatedDate
        );

        return Result.Success(dto);
    }
}
