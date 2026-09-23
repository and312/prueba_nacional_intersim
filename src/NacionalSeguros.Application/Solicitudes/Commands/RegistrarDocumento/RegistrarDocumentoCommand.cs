using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.RegistrarDocumento;

public record RegistrarDocumentoCommand(
    int SolicitudId,
    SolicitudDocumentoDto Dto
) : IRequest<Result>;
