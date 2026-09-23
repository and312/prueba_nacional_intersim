using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.ActualizarParametro;

public class ActualizarParametroCommandHandler : IRequestHandler<ActualizarParametroCommand, Result<ParametroResponse>>
{
    private readonly IParametroRepository _parametroRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICatalogoCacheService _cacheService;

    public ActualizarParametroCommandHandler(
        IParametroRepository parametroRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICatalogoCacheService cacheService)
    {
        _parametroRepository = parametroRepository ?? throw new ArgumentNullException(nameof(parametroRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<Result<ParametroResponse>> Handle(ActualizarParametroCommand request, CancellationToken cancellationToken)
    {
        var parametro = await _parametroRepository.GetByIdAsync(request.Id);
        if (parametro == null)
        {
            return Result.Failure<ParametroResponse>(new Error("Parametro.NotFound", "El parámetro especificado no existe."));
        }

        if (request.ParametroIdPadre.HasValue)
        {
            // Validar que no se asigne a sí mismo
            if (request.ParametroIdPadre.Value == parametro.Id)
            {
                return Result.Failure<ParametroResponse>(new Error("CIRCULAR_DEPENDENCY_DETECTED", "Se ha detectado una dependencia circular en la jerarquía del parámetro."));
            }

            var padre = await _parametroRepository.GetByIdAsync(request.ParametroIdPadre.Value);
            if (padre == null)
            {
                return Result.Failure<ParametroResponse>(new Error("Parametro.ParentNotFound", "El parámetro padre especificado no existe."));
            }

            // El padre debe pertenecer al mismo catálogo
            if (padre.CatalogoId != parametro.CatalogoId)
            {
                return Result.Failure<ParametroResponse>(new Error("Parametro.InvalidParentCatalog", "El parámetro padre debe pertenecer al mismo catálogo."));
            }

            // Validar dependencias circulares (recursividad)
            bool isCircular = await _parametroRepository.HasCircularDependencyAsync(parametro.Id, request.ParametroIdPadre.Value);
            if (isCircular)
            {
                return Result.Failure<ParametroResponse>(new Error("CIRCULAR_DEPENDENCY_DETECTED", "Se ha detectado una dependencia circular en la jerarquía del parámetro."));
            }
        }

        string estadoAnterior = $"Valor: {parametro.Valor}, PadreId: {parametro.ParametroIdPadre}";

        parametro.Actualizar(request.Valor, request.ParametroIdPadre);

        _parametroRepository.Update(parametro);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalida la caché del catálogo
        await _cacheService.InvalidateCacheAsync(parametro.Catalogo.Codigo);



        var dto = _mapper.Map<ParametroResponse>(parametro);
        return Result.Success(dto);
    }
}
