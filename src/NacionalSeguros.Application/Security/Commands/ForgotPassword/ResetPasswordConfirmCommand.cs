using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.ForgotPassword;

public record ResetPasswordConfirmCommand(
    int UserId, 
    string Token, 
    string NuevaClave) : IRequest<Result>;
