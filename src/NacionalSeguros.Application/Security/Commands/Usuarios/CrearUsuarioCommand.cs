using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public record CrearUsuarioCommand(
    string Correo,
    string Nombres,
    string Apellidos,
    string TipoAutenticacion,
    string? Clave,
    int AreaId,
    string? Cargo = null,
    string? Gerencia = null,
    string? Telefono = null,
    string? Extension = null,
    string? Observaciones = null,
    string? FotografiaUrl = null,
    System.Collections.Generic.List<int>? RolIds = null) : IRequest<Result<UsuarioResponseDto>>;
