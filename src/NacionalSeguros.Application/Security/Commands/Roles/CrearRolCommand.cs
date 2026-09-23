using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Roles;

public record CrearRolCommand(
    string Nombre,
    string? Descripcion,
    List<string> PermisoCodigos) : IRequest<Result<RolResponseDto>>;
