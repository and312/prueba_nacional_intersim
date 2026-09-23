using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Matchings.Commands.EjecutarMatching;

public record EjecutarMatchingCommand(
    int PostulanteId,
    int VacanteId,
    string ExecutedBy) : IRequest<Result<MatchingResultDto>>;
