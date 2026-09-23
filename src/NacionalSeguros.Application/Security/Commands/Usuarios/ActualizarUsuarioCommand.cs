using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public record ActualizarUsuarioCommand(
    int UsuarioId,
    string Correo,
    string Nombres,
    string Apellidos,
    int AreaId,
    string? Cargo,
    string? Gerencia,
    string? Telefono,
    string? Extension,
    string? Observaciones,
    string? FotografiaUrl,
    System.Collections.Generic.List<int> RolIds,
    string Estado) : IRequest<Result<UsuarioResponseDto>>;
