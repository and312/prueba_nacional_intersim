using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.ActualizarCatalogo;

public record ActualizarCatalogoCommand(int Id, string Nombre, string ModifiedBy) : IRequest<Result<CatalogoResponse>>;
