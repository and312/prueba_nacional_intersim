using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.ActualizarPerfilSeccion;

public record ActualizarPerfilSeccionCommand(
    int PerfilCargoId,
    int NumeroSeccion,
    string Contenido,
    string Usuario,
    string? Motivo
) : IRequest<Result<PerfilResponseDto>>;
