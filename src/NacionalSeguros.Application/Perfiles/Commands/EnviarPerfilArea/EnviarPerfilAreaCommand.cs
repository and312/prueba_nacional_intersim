using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.EnviarPerfilArea;

public record EnviarPerfilAreaCommand(
    int PerfilCargoId,
    string? Comentario,
    string UserEmail
) : IRequest<Result<PerfilResponseDto>>;
