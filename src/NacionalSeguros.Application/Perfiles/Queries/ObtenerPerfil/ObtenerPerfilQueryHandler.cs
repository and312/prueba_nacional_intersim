using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfil;

public class ObtenerPerfilQueryHandler : IRequestHandler<ObtenerPerfilQuery, Result<PerfilResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IMapper _mapper;

    public ObtenerPerfilQueryHandler(IPerfilCargoRepository perfilCargoRepository, IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PerfilResponseDto>> Handle(ObtenerPerfilQuery request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilCargoRepository.GetLatestBySolicitudIdAsync(request.SolicitudId);
        if (perfil == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.NotFound", $"No existe ningún perfil generado para la solicitud con ID {request.SolicitudId}."));
        }

        var responseDto = _mapper.Map<PerfilResponseDto>(perfil);
        return Result.Success(responseDto);
    }
}
