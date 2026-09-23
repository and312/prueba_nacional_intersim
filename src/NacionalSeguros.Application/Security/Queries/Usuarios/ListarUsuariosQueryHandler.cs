using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.Usuarios;

public class ListarUsuariosQueryHandler : IRequestHandler<ListarUsuariosQuery, Result<PagedUsuariosResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;

    public ListarUsuariosQueryHandler(IUsuarioRepository usuarioRepository, IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedUsuariosResponseDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (items, totalCount) = await _usuarioRepository.GetPagedAsync(
            pageNumber, 
            pageSize, 
            request.Search,
            request.AreaId,
            request.RolId,
            request.Estado,
            request.Cargo,
            request.Gerencia);

        var dtos = _mapper.Map<List<UsuarioResponseDto>>(items);
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedDto = new PagedUsuariosResponseDto
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            TotalPages = totalPages
        };

        return Result.Success(pagedDto);
    }
}
