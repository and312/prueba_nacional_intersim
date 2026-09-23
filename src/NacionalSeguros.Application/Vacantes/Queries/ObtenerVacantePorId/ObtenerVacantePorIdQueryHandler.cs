using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Queries.ObtenerVacantePorId;

public class ObtenerVacantePorIdQueryHandler : IRequestHandler<ObtenerVacantePorIdQuery, Result<VacanteResponseDto>>
{
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IMapper _mapper;

    public ObtenerVacantePorIdQueryHandler(IVacanteRepository vacanteRepository, IMapper mapper)
    {
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<VacanteResponseDto>> Handle(ObtenerVacantePorIdQuery request, CancellationToken cancellationToken)
    {
        var vacante = await _vacanteRepository.GetByIdAsync(request.VacanteId);
        if (vacante == null)
        {
            return Result.Failure<VacanteResponseDto>(new Error("Vacante.NotFound", $"La vacante con ID {request.VacanteId} no existe."));
        }

        var dto = _mapper.Map<VacanteResponseDto>(vacante);
        return Result.Success(dto);
    }
}
