using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.Areas;

public record ObtenerAreasQuery() : IRequest<Result<List<AreaResponseDto>>>;
