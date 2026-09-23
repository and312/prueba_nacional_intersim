using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.ObtenerCatalogos;

public record ObtenerCatalogosQuery() : IRequest<Result<IEnumerable<CatalogoResponse>>>;
