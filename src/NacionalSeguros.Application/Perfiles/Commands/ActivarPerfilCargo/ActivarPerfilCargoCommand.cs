using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.ActivarPerfilCargo;

public record ActivarPerfilCargoCommand(int PerfilCargoId, string ModifiedBy) : IRequest<Result>;
