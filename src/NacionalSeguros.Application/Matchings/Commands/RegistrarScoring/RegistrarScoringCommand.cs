using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Matchings.Commands.RegistrarScoring;

public record RegistrarScoringCommand(
    ScoringCallbackRequestDto Dto) : IRequest<Result<ScoringResultDto>>;
