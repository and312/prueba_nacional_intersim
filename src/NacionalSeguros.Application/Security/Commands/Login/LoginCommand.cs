using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Login;

public record LoginCommand(
    string Correo,
    string Clave,
    string TipoAutenticacion) : IRequest<Result<LoginResponseDto>>;
