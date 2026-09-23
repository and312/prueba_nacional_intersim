using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.ActualizarCatalogo;

public class ActualizarCatalogoCommandHandler : IRequestHandler<ActualizarCatalogoCommand, Result<CatalogoResponse>>
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICatalogoCacheService _cacheService;

    public ActualizarCatalogoCommandHandler(
        ICatalogoRepository catalogoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICatalogoCacheService cacheService)
    {
        _catalogoRepository = catalogoRepository ?? throw new ArgumentNullException(nameof(catalogoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<Result<CatalogoResponse>> Handle(ActualizarCatalogoCommand request, CancellationToken cancellationToken)
    {
        var catalogo = await _catalogoRepository.GetByIdAsync(request.Id);
        if (catalogo == null)
        {
            return Result.Failure<CatalogoResponse>(new Error("Catalogo.NotFound", "El catálogo especificado no existe."));
        }

        string estadoAnterior = $"Nombre: {catalogo.Nombre}";

        catalogo.Actualizar(request.Nombre);

        _catalogoRepository.Update(catalogo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalida la caché del catálogo
        await _cacheService.InvalidateCacheAsync(catalogo.Codigo);



        var dto = _mapper.Map<CatalogoResponse>(catalogo);
        return Result.Success(dto);
    }
}
