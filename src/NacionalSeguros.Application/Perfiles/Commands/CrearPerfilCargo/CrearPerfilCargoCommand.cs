using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.CrearPerfilCargo;

public record CrearPerfilCargoCommand(
    PerfilEstructuradoInputDto Dto,
    string CreatedBy
) : IRequest<Result<PerfilResponseDto>>;
