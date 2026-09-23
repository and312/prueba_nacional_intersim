using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.BuscarCatalogos;

public record BuscarCatalogosQuery(string SearchTerm) : IRequest<Result<IEnumerable<CatalogoResponse>>>;
