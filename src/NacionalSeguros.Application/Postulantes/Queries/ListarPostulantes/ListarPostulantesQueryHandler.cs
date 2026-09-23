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

namespace NacionalSeguros.Application.Postulantes.Queries.ListarPostulantes;

public class ListarPostulantesQueryHandler : IRequestHandler<ListarPostulantesQuery, Result<PagedPostulantesResponseDto>>
{
    private readonly IPostulanteRepository _postulanteRepository;
    private readonly IMapper _mapper;

    public ListarPostulantesQueryHandler(IPostulanteRepository postulanteRepository, IMapper mapper)
    {
        _postulanteRepository = postulanteRepository ?? throw new ArgumentNullException(nameof(postulanteRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PagedPostulantesResponseDto>> Handle(ListarPostulantesQuery request, CancellationToken cancellationToken)
    {
        var postulaciones = await _postulanteRepository.ListPostulacionesAsync(request.VacanteId);

        int totalCount = postulaciones.Count();

        var paginated = postulaciones
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = paginated.Select(p => new PostulanteResponseDto(
            PostulanteId: p.PostulanteId,
            Correo: p.Postulante != null ? p.Postulante.Correo : string.Empty,
            EstadoNombre: p.EstadoPipeline != null ? p.EstadoPipeline.Nombre : "Registrado",
            FechaRegistro: p.FechaPostulacion
        )).ToList();

        var result = new PagedPostulantesResponseDto(dtos, totalCount);
        return Result.Success(result);
    }
}
