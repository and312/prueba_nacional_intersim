using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.Usuarios;

public class ObtenerUsuarioPorIdQueryHandler : IRequestHandler<ObtenerUsuarioPorIdQuery, Result<UsuarioResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;

    public ObtenerUsuarioPorIdQueryHandler(IUsuarioRepository usuarioRepository, IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<Result<UsuarioResponseDto>> Handle(ObtenerUsuarioPorIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);

        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure<UsuarioResponseDto>(new Error("USER_NOT_FOUND", "El usuario no existe o ha sido eliminado."));
        }

        var dto = _mapper.Map<UsuarioResponseDto>(usuario);
        return Result.Success(dto);
    }
}
