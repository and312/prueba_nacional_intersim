using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Queries.ObtenerVacantePorId;

public record ObtenerVacantePorIdQuery(int VacanteId) : IRequest<Result<VacanteResponseDto>>;
