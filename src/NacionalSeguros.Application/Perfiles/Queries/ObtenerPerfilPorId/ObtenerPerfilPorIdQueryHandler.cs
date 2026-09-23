using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfilPorId;

public class ObtenerPerfilPorIdQueryHandler : IRequestHandler<ObtenerPerfilPorIdQuery, Result<PerfilResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IMapper _mapper;

    public ObtenerPerfilPorIdQueryHandler(IPerfilCargoRepository perfilCargoRepository, IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PerfilResponseDto>> Handle(ObtenerPerfilPorIdQuery request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilCargoRepository.GetByIdAsync(request.PerfilCargoId);
        if (perfil == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.NotFound", $"El perfil de cargo con ID {request.PerfilCargoId} no existe."));
        }

        if (!perfil.Activo)
        {
            var activePerfil = await _perfilCargoRepository.GetLatestBySolicitudIdAsync(perfil.SolicitudId);
            if (activePerfil != null)
            {
                perfil = activePerfil;
            }
        }

        var responseDto = _mapper.Map<PerfilResponseDto>(perfil);
        return Result.Success(responseDto);
    }
}
