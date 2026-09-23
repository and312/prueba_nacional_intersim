using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Matchings.Queries.ObtenerResultadoMatching;

public record ObtenerResultadoMatchingQuery(
    int PostulanteId,
    int VacanteId) : IRequest<Result<MatchingExpedienteDto>>;
