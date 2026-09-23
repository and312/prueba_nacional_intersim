using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.EliminarCatalogo;

public record EliminarCatalogoCommand(int Id, string DeletedBy) : IRequest<Result>;
