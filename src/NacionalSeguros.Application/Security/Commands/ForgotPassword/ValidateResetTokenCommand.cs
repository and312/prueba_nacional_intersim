using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.ForgotPassword;

public record ValidateResetTokenCommand(int UserId, string Token) : IRequest<Result>;
