using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.EliminarParametro;

public class EliminarParametroCommandHandler : IRequestHandler<EliminarParametroCommand, Result>
{
    private readonly IParametroRepository _parametroRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICatalogoCacheService _cacheService;

    public EliminarParametroCommandHandler(
        IParametroRepository parametroRepository,
        IUnitOfWork unitOfWork,
        ICatalogoCacheService cacheService)
    {
        _parametroRepository = parametroRepository ?? throw new ArgumentNullException(nameof(parametroRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<Result> Handle(EliminarParametroCommand request, CancellationToken cancellationToken)
    {
        var parametro = await _parametroRepository.GetByIdAsync(request.Id);
        if (parametro == null)
        {
            return Result.Failure(new Error("Parametro.NotFound", "El parámetro especificado no existe."));
        }

        // Validar si el parámetro está en uso activo (H-01)
        bool inUse = await _parametroRepository.IsInUseAsync(request.Id);
        if (inUse)
        {
            return Result.Failure(new Error("Parametro.InUse", "No se puede inactivar el parámetro porque está siendo utilizado en el sistema."));
        }

        string estadoAnterior = $"IsDeleted: {parametro.IsDeleted}";

        parametro.EliminarLogicamente();

        _parametroRepository.Update(parametro);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalida la caché del catálogo
        await _cacheService.InvalidateCacheAsync(parametro.Catalogo.Codigo);

        return Result.Success();
    }
}
