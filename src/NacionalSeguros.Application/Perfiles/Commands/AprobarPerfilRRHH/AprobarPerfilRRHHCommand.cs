using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.AprobarPerfilRRHH;

public record AprobarPerfilRRHHCommand(
    int PerfilCargoId,
    string? Comentario,
    string UserEmail
) : IRequest<Result<PerfilResponseDto>>;
