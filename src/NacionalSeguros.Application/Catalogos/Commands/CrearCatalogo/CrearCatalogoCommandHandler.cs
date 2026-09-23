using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.CrearCatalogo;

public class CrearCatalogoCommandHandler : IRequestHandler<CrearCatalogoCommand, Result<CatalogoResponse>>
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CrearCatalogoCommandHandler(
        ICatalogoRepository catalogoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _catalogoRepository = catalogoRepository ?? throw new ArgumentNullException(nameof(catalogoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<CatalogoResponse>> Handle(CrearCatalogoCommand request, CancellationToken cancellationToken)
    {
        var existente = await _catalogoRepository.GetByCodigoAsync(request.Codigo);
        if (existente != null)
        {
            return Result.Failure<CatalogoResponse>(new Error("Catalogo.DuplicateCode", "Ya existe un catálogo con el código especificado."));
        }

        var catalogo = new Catalogo(request.Nombre, request.Codigo, request.CreatedBy);

        await _catalogoRepository.AddAsync(catalogo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);



        var dto = _mapper.Map<CatalogoResponse>(catalogo);
        return Result.Success(dto);
    }
}
