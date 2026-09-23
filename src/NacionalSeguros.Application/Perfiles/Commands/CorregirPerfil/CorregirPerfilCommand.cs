using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.CorregirPerfil;

public record CorregirPerfilCommand(
    int PerfilCargoId,
    string Cargo,
    string Descripcion,
    string UserEmail
) : IRequest<Result<PerfilResponseDto>>;
