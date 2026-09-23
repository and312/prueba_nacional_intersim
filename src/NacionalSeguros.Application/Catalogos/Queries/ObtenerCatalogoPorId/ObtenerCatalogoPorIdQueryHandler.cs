using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.ObtenerCatalogoPorId;

public class ObtenerCatalogoPorIdQueryHandler : IRequestHandler<ObtenerCatalogoPorIdQuery, Result<CatalogoResponse>>
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IMapper _mapper;

    public ObtenerCatalogoPorIdQueryHandler(ICatalogoRepository catalogoRepository, IMapper mapper)
    {
        _catalogoRepository = catalogoRepository ?? throw new ArgumentNullException(nameof(catalogoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<CatalogoResponse>> Handle(ObtenerCatalogoPorIdQuery request, CancellationToken cancellationToken)
    {
        var catalogo = await _catalogoRepository.GetByIdAsync(request.Id);
        if (catalogo == null)
        {
            return Result.Failure<CatalogoResponse>(new Error("Catalogo.NotFound", "El catálogo especificado no existe."));
        }

        var dto = _mapper.Map<CatalogoResponse>(catalogo);
        return Result.Success(dto);
    }
}
