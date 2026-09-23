using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.GuardarResumen;

public record GuardarResumenCommand(
    int Id,
    SolicitudResumenRequestDto Dto
) : IRequest<Result>;
