using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.ConfigurarMfa;

public record ConfigurarMfaCommand(int UsuarioId) : IRequest<Result<ConfigurarMfaResponse>>;

public record ConfigurarMfaResponse(string MfaSecreto, string QrCodeUri);
