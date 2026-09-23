using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Queries.ListarPostulantes;

public record ListarPostulantesQuery(
    int PageNumber,
    int PageSize,
    int? VacanteId) : IRequest<Result<PagedPostulantesResponseDto>>;
