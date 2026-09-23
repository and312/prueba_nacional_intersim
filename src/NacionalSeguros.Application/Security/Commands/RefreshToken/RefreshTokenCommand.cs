using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.RefreshToken;

public record RefreshTokenCommand(
    string TokenExpirado,
    string RefreshToken) : IRequest<Result<LoginResponseDto>>;
