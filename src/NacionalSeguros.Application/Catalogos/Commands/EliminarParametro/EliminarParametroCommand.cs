using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.EliminarParametro;

public record EliminarParametroCommand(int Id, string DeletedBy) : IRequest<Result>;
