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

namespace NacionalSeguros.Application.Perfiles.Queries.BuscarPerfiles;

public class BuscarPerfilesQueryHandler : IRequestHandler<BuscarPerfilesQuery, Result<IEnumerable<PerfilResponseDto>>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IMapper _mapper;

    public BuscarPerfilesQueryHandler(IPerfilCargoRepository perfilCargoRepository, IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<IEnumerable<PerfilResponseDto>>> Handle(BuscarPerfilesQuery request, CancellationToken cancellationToken)
    {
        var perfiles = await _perfilCargoRepository.ListAsync();

        // 1. Agrupar por SolicitudId y seleccionar solo la versión más reciente de cada perfil
        var perfilesAgrupados = perfiles
            .GroupBy(p => p.SolicitudId)
            .Select(g => g.OrderByDescending(p => p.Version).First())
            .ToList();

        // 2. Filtrar para mostrar únicamente perfiles en estado de revisión/corrección de RRHH
        var perfilesRRHH = perfilesAgrupados
            .Where(p => p.Estado?.Codigo == "EnRevisionRRHH" || 
                        p.Estado?.Codigo == "PerfilCorregidoRRHH" ||
                        p.Estado?.Codigo == "PERF-REV-RRHH" ||
                        p.Estado?.Codigo == "PERF-COR-RRHH" ||
                        p.Estado?.Codigo == "PERF-RES-GEN");

        // 3. Aplicar filtro de búsqueda si el término está presente
        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim().ToLower();
            perfilesRRHH = perfilesRRHH.Where(p => 
                (p.Cargo ?? string.Empty).ToLower().Contains(term) ||
                (p.Descripcion ?? string.Empty).ToLower().Contains(term) ||
                (p.Solicitud?.Solicitante?.Area?.Nombre ?? string.Empty).ToLower().Contains(term) ||
                (p.Solicitud?.Solicitante?.Correo ?? string.Empty).ToLower().Contains(term) ||
                (p.Solicitud?.Codigo ?? string.Empty).ToLower().Contains(term)
            );
        }

        var responseDtos = _mapper.Map<IEnumerable<PerfilResponseDto>>(perfilesRRHH.ToList());
        return Result.Success(responseDtos);
    }
}
