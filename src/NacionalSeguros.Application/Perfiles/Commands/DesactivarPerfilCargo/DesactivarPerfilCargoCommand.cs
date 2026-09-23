using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.DesactivarPerfilCargo;

public record DesactivarPerfilCargoCommand(int PerfilCargoId, string ModifiedBy) : IRequest<Result>;
