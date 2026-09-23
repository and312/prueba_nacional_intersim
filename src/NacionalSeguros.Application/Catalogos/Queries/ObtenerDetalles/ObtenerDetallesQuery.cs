using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.ObtenerDetalles;

public record ObtenerDetallesQuery(int? CatalogoId, string? CatalogoCodigo) : IRequest<Result<IEnumerable<ParametroResponse>>>;
