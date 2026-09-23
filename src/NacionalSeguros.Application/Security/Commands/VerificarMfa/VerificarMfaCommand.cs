using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.VerificarMfa;

public record VerificarMfaCommand(
    string Correo,
    string CodigoOtp) : IRequest<Result<LoginResponseDto>>;
