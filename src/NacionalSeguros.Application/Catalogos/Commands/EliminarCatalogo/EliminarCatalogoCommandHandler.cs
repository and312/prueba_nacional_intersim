using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.EliminarCatalogo;

public class EliminarCatalogoCommandHandler : IRequestHandler<EliminarCatalogoCommand, Result>
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICatalogoCacheService _cacheService;

    public EliminarCatalogoCommandHandler(
        ICatalogoRepository catalogoRepository,
        IUnitOfWork unitOfWork,
        ICatalogoCacheService cacheService)
    {
        _catalogoRepository = catalogoRepository ?? throw new ArgumentNullException(nameof(catalogoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<Result> Handle(EliminarCatalogoCommand request, CancellationToken cancellationToken)
    {
        var catalogo = await _catalogoRepository.GetByIdAsync(request.Id);
        if (catalogo == null)
        {
            return Result.Failure(new Error("Catalogo.NotFound", "El catálogo especificado no existe."));
        }

        // Validar si el catálogo está en uso activo (H-01)
        bool inUse = await _catalogoRepository.IsInUseAsync(request.Id);
        if (inUse)
        {
            return Result.Failure(new Error("Catalogo.InUse", "No se puede inactivar el catálogo porque tiene parámetros que están siendo utilizados en el sistema."));
        }

        string estadoAnterior = $"IsDeleted: {catalogo.IsDeleted}";

        catalogo.EliminarLogicamente();

        _catalogoRepository.Update(catalogo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalida la caché del catálogo
        await _cacheService.InvalidateCacheAsync(catalogo.Codigo);

        return Result.Success();
    }
}
