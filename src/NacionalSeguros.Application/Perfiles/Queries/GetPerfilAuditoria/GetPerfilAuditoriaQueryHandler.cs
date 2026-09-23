using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.GetPerfilAuditoria;

public class GetPerfilAuditoriaQueryHandler : IRequestHandler<GetPerfilAuditoriaQuery, Result<IEnumerable<PerfilAuditoriaResponseDto>>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IMapper _mapper;

    public GetPerfilAuditoriaQueryHandler(IPerfilCargoRepository perfilCargoRepository, IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<IEnumerable<PerfilAuditoriaResponseDto>>> Handle(GetPerfilAuditoriaQuery request, CancellationToken cancellationToken)
    {
        var auditorias = await _perfilCargoRepository.GetAuditoriasByPerfilIdAsync(request.PerfilCargoId);
        var mapped = _mapper.Map<IEnumerable<PerfilAuditoriaResponseDto>>(auditorias);
        return Result.Success(mapped);
    }
}
