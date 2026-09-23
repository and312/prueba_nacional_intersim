using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.ActualizarPerfilCargo;

public record ActualizarPerfilCargoCommand(
    int PerfilCargoId,
    string Cargo,
    string Descripcion,
    string ModifiedBy
) : IRequest<Result<PerfilResponseDto>>;
