using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Matchings.Queries.ObtenerResultadoMatching;

public class ObtenerResultadoMatchingQueryHandler : IRequestHandler<ObtenerResultadoMatchingQuery, Result<MatchingExpedienteDto>>
{
    private readonly IMatchingRepository _matchingRepository;

    public ObtenerResultadoMatchingQueryHandler(IMatchingRepository matchingRepository)
    {
        _matchingRepository = matchingRepository ?? throw new ArgumentNullException(nameof(matchingRepository));
    }

    public async Task<Result<MatchingExpedienteDto>> Handle(ObtenerResultadoMatchingQuery request, CancellationToken cancellationToken)
    {
        var matching = await _matchingRepository.GetLatestMatchingAsync(request.PostulanteId, request.VacanteId);
        var scoring = await _matchingRepository.GetLatestScoringAsync(request.PostulanteId, request.VacanteId);

        if (matching == null && scoring == null)
        {
            return Result.Failure<MatchingExpedienteDto>(new Error("Matching.NotFound", $"No existen registros de matching o scoring para el postulante {request.PostulanteId} y vacante {request.VacanteId}."));
        }

        var matchingDto = matching != null ? new MatchingResultDto(
            PostulanteId: matching.PostulanteId,
            VacanteId: matching.VacanteId,
            ScoreCoincidencia: matching.ScoreCoincidencia,
            CoincidenciasText: matching.CoincidenciasText,
            BrechasText: matching.BrechasText,
            FechaEvaluacion: matching.CreatedDate
        ) : null;

        var scoringDto = scoring != null ? new ScoringResultDto(
            PostulanteId: scoring.PostulanteId,
            VacanteId: scoring.VacanteId,
            ScoreSkills: scoring.ScoreSkills,
            ScoreExperiencia: scoring.ScoreExperiencia,
            ScoreFinal: scoring.ScoreFinal,
            JustificacionText: scoring.JustificacionText,
            FechaEvaluacion: scoring.CreatedDate
        ) : null;

        var result = new MatchingExpedienteDto(matchingDto, scoringDto);
        return Result.Success(result);
    }
}
