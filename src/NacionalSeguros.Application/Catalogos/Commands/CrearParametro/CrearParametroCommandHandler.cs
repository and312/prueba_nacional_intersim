using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.CrearParametro;

public class CrearParametroCommandHandler : IRequestHandler<CrearParametroCommand, Result<ParametroResponse>>
{
    private readonly IParametroRepository _parametroRepository;
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICatalogoCacheService _cacheService;

    public CrearParametroCommandHandler(
        IParametroRepository parametroRepository,
        ICatalogoRepository catalogoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICatalogoCacheService cacheService)
    {
        _parametroRepository = parametroRepository ?? throw new ArgumentNullException(nameof(parametroRepository));
        _catalogoRepository = catalogoRepository ?? throw new ArgumentNullException(nameof(catalogoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<Result<ParametroResponse>> Handle(CrearParametroCommand request, CancellationToken cancellationToken)
    {
        var catalogo = await _catalogoRepository.GetByIdAsync(request.CatalogoId);
        if (catalogo == null)
        {
            return Result.Failure<ParametroResponse>(new Error("Catalogo.NotFound", "El catálogo especificado no existe."));
        }

        var existente = await _parametroRepository.GetByCodigoAsync(request.CatalogoId, request.Codigo);
        if (existente != null && !existente.IsDeleted)
        {
            return Result.Failure<ParametroResponse>(new Error("Parametro.DuplicateCode", "Ya existe un parámetro con el código especificado en este catálogo."));
        }

        if (request.ParametroIdPadre.HasValue)
        {
            var padre = await _parametroRepository.GetByIdAsync(request.ParametroIdPadre.Value);
            if (padre == null)
            {
                return Result.Failure<ParametroResponse>(new Error("Parametro.ParentNotFound", "El parámetro padre especificado no existe."));
            }

            // Validar dependencias circulares (recursividad)
            bool isCircular = await _parametroRepository.HasCircularDependencyAsync(0, request.ParametroIdPadre.Value);
            if (isCircular)
            {
                return Result.Failure<ParametroResponse>(new Error("CIRCULAR_DEPENDENCY_DETECTED", "Se ha detectado una dependencia circular en la jerarquía del parámetro."));
            }
        }

        var parametro = new Parametro(
            request.CatalogoId,
            request.Codigo,
            request.Valor,
            request.ParametroIdPadre,
            request.CreatedBy);

        await _parametroRepository.AddAsync(parametro);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalida la caché del catálogo
        await _cacheService.InvalidateCacheAsync(catalogo.Codigo);



        var dto = _mapper.Map<ParametroResponse>(parametro);
        return Result.Success(dto);
    }
}
