using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.Gerencias;

public class ObtenerGerenciasQueryHandler : IRequestHandler<ObtenerGerenciasQuery, Result<List<GerenciaResponseDto>>>
{
    private readonly IGerenciaRepository _gerenciaRepository;
    private readonly IMapper _mapper;

    public ObtenerGerenciasQueryHandler(IGerenciaRepository gerenciaRepository, IMapper _mapper)
    {
        _gerenciaRepository = gerenciaRepository;
        this._mapper = _mapper;
    }

    public async Task<Result<List<GerenciaResponseDto>>> Handle(ObtenerGerenciasQuery request, CancellationToken cancellationToken)
    {
        var gerencias = await _gerenciaRepository.GetAllActiveAsync();
        var dtos = _mapper.Map<List<GerenciaResponseDto>>(gerencias.ToList());
        return Result.Success(dtos);
    }
}
