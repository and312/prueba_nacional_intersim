using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Matchings.Commands.RegistrarScoring;

public class RegistrarScoringCommandHandler : IRequestHandler<RegistrarScoringCommand, Result<ScoringResultDto>>
{
    private readonly IMatchingRepository _matchingRepository;
    private readonly IPostulanteRepository _postulanteRepository;
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarScoringCommandHandler(
        IMatchingRepository matchingRepository,
        IPostulanteRepository postulanteRepository,
        IVacanteRepository vacanteRepository,
        IUnitOfWork unitOfWork)
    {
        _matchingRepository = matchingRepository ?? throw new ArgumentNullException(nameof(matchingRepository));
        _postulanteRepository = postulanteRepository ?? throw new ArgumentNullException(nameof(postulanteRepository));
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ScoringResultDto>> Handle(RegistrarScoringCommand request, CancellationToken cancellationToken)
    {
        var postulante = await _postulanteRepository.GetByIdAsync(request.Dto.PostulanteId);
        if (postulante == null)
        {
            return Result.Failure<ScoringResultDto>(new Error("Postulante.NotFound", $"El postulante con ID {request.Dto.PostulanteId} no existe."));
        }

        var vacante = await _vacanteRepository.GetByIdAsync(request.Dto.VacanteId);
        if (vacante == null)
        {
            return Result.Failure<ScoringResultDto>(new Error("Vacante.NotFound", $"La vacante con ID {request.Dto.VacanteId} no existe."));
        }

        DateTime ahora = DateTime.UtcNow;

        // Registrar la ejecución de inferencia del agente de scoring
        var execution = new AgentExecution(
            agenteId: 5, // Agente de Scoring
            promptVersionId: 1,
            usuarioId: 1, // System/n8n
            fechaInicio: ahora.AddSeconds(-2),
            fechaFin: ahora,
            duracionMs: 2000,
            inputJson: JsonSerializer.Serialize(new { VacanteId = vacante.Id, PostulanteId = postulante.Id }),
            outputJson: JsonSerializer.Serialize(request.Dto),
            resultadoStatus: "Success",
            tokensInput: 1500,
            tokensOutput: 500,
            costoEstimado: 0.03m,
            correlationId: Guid.NewGuid()
        );

        await _matchingRepository.AddAgentExecutionAsync(execution);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var scoring = new Scoring(
            postulanteId: postulante.Id,
            vacanteId: vacante.Id,
            scoreSkills: request.Dto.ScoreTecnico,
            scoreExperiencia: request.Dto.ScoreExperiencia,
            scoreFinal: request.Dto.ScoreGeneral,
            justificacionText: request.Dto.ExplicabilidadDetalle,
            executionId: execution.ExecutionId
        );

        await _matchingRepository.AddScoringAsync(scoring);

        _unitOfWork.TransitionComment = $"Notas de idoneidad y scoring de IA registradas para candidato {postulante.Id} y vacante {vacante.Id}.";
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new ScoringResultDto(
            PostulanteId: scoring.PostulanteId,
            VacanteId: scoring.VacanteId,
            ScoreSkills: scoring.ScoreSkills,
            ScoreExperiencia: scoring.ScoreExperiencia,
            ScoreFinal: scoring.ScoreFinal,
            JustificacionText: scoring.JustificacionText,
            FechaEvaluacion: scoring.CreatedDate
        );

        return Result.Success(dto);
    }
}
