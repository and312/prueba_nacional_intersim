using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Queries.BuscarVacantes;

public record BuscarVacantesQuery(
    int PageNumber,
    int PageSize) : IRequest<Result<PagedVacantesResponseDto>>;
