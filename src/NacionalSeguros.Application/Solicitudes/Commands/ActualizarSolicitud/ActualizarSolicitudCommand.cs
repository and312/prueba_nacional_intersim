using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.ActualizarSolicitud;

public record ActualizarSolicitudCommand(int Id, SolicitudUpdateDto Dto, string ModifiedBy, string CanalOrigen = "BackOffice") : IRequest<Result<SolicitudResponseDto>>;
