using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

using System;

namespace NacionalSeguros.Application.Solicitudes.Commands.CrearSolicitud;

public record CrearSolicitudCommand(
    SolicitudCreateDto Dto,
    string CreatedBy,
    int? UsuarioId = null,
    string CanalOrigen = "BackOffice",
    string? WorkflowOrigen = null,
    long? ApiKeyId = null,
    Guid? CorrelationId = null) : IRequest<Result<SolicitudResponseDto>>;
