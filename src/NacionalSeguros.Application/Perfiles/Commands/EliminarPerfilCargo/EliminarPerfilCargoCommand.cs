using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.EliminarPerfilCargo;

public record EliminarPerfilCargoCommand(int PerfilCargoId) : IRequest<Result>;
