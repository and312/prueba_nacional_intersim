using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.AprobarPerfilCargo;

public record AprobarPerfilCargoCommand(
    int PerfilCargoId,
    string ModifiedBy
) : IRequest<Result<PerfilResponseDto>>;
