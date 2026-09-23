using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
