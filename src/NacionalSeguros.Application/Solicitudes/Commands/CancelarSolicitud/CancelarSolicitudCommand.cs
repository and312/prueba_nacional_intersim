using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.CancelarSolicitud;

public record CancelarSolicitudCommand(int Id, string Motivo, string ModifiedBy) : IRequest<Result<SolicitudResponseDto>>;
