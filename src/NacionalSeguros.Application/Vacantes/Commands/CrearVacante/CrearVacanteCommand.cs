using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.CrearVacante;

public record CrearVacanteCommand(
    VacanteCreateDto Dto,
    string CreatedBy) : IRequest<Result<VacanteResponseDto>>;
