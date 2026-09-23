using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.CrearCatalogo;

public record CrearCatalogoCommand(string Nombre, string Codigo, string CreatedBy) : IRequest<Result<CatalogoResponse>>;
