using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Correo, string FrontendBaseUrl) : IRequest<Result>;
