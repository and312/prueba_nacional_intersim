using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.ObtenerRoles;

public record ObtenerRolesQuery : IRequest<Result<List<RolResponseDto>>>;
