using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.ObtenerPermisos;

public record ObtenerPermisosQuery : IRequest<Result<List<string>>>;
