using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.ObservarPerfilCargo;

public record ObservarPerfilCargoCommand(
    int PerfilCargoId,
    string Justificacion,
    string ModifiedBy
) : IRequest<Result<PerfilResponseDto>>;
