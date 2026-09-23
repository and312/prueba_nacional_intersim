using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.TransitarSolicitud;

public record TransitarSolicitudEstadoCommand(
    int Id,
    string NuevoEstadoCodigo,
    string? Comentario,
    int? DecisorId,
    string ModifiedBy,
    bool? SeMostroAdvertencia = null,
    bool? UsuarioConfirmoEnvio = null) : IRequest<Result<SolicitudResponseDto>>;
