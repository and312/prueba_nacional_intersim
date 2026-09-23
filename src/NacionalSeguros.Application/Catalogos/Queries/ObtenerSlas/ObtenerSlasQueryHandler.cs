using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.ObtenerSlas;

public class ObtenerSlasQueryHandler : IRequestHandler<ObtenerSlasQuery, Result<IEnumerable<SlaResponse>>>
{
    private readonly ISlaRepository _slaRepository;
    private readonly IMapper _mapper;

    public ObtenerSlasQueryHandler(ISlaRepository slaRepository, IMapper mapper)
    {
        _slaRepository = slaRepository ?? throw new ArgumentNullException(nameof(slaRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<IEnumerable<SlaResponse>>> Handle(ObtenerSlasQuery request, CancellationToken cancellationToken)
    {
        var slas = await _slaRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<SlaResponse>>(slas);
        return Result.Success(dtos);
    }
}
