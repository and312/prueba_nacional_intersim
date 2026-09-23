using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.EnviarSolicitudARRHH;

public record EnviarSolicitudARRHHCommand(
    int Id,
    string UserEmail) : IRequest<Result<SolicitudResponseDto>>;
