using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.GetPerfilTrazabilidad;

public class GetPerfilTrazabilidadQueryHandler : IRequestHandler<GetPerfilTrazabilidadQuery, Result<List<StateHistoryResponseDto>>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IMapper _mapper;

    public GetPerfilTrazabilidadQueryHandler(IPerfilCargoRepository perfilCargoRepository, IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<List<StateHistoryResponseDto>>> Handle(GetPerfilTrazabilidadQuery request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilCargoRepository.GetByIdAsync(request.PerfilCargoId);
        if (perfil == null)
        {
            return Result.Failure<List<StateHistoryResponseDto>>(new Error("PerfilCargo.NotFound", $"El perfil de cargo con ID {request.PerfilCargoId} no existe."));
        }

        // Obtener todos los perfiles asociados a la misma solicitud para incluir todas las versiones
        var perfilesAsociados = (await _perfilCargoRepository.ListAsync())
            .Where(p => p.SolicitudId == perfil.SolicitudId)
            .ToList();

        var perfilIds = perfilesAsociados.Select(p => p.Id).ToList();

        var history = await _perfilCargoRepository.GetStateHistoryForProfilesAsync(perfilIds);

        var responseDtos = _mapper.Map<List<StateHistoryResponseDto>>(history.ToList());
        return Result.Success(responseDtos);
    }
}
