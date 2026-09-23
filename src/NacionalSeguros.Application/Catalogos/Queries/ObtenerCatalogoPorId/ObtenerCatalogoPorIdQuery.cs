using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.ObtenerCatalogoPorId;

public record ObtenerCatalogoPorIdQuery(int Id) : IRequest<Result<CatalogoResponse>>;
