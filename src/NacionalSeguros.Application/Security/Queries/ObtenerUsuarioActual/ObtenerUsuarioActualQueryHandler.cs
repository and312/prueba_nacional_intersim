using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.ObtenerUsuarioActual;

public class ObtenerUsuarioActualQueryHandler : IRequestHandler<ObtenerUsuarioActualQuery, Result<UsuarioResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObtenerUsuarioActualQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<UsuarioResponseDto>> Handle(ObtenerUsuarioActualQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);

        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure<UsuarioResponseDto>(new Error("USER_NOT_FOUND", "El usuario especificado no existe."));
        }

        var dto = new UsuarioResponseDto
        {
            UsuarioId = usuario.Id,
            Correo = usuario.Correo,
            Nombres = usuario.Nombre,
            TipoAutenticacion = usuario.TipoAutenticacion.ToString(),
            Activo = usuario.Estado == UsuarioEstado.Activo,
            Roles = usuario.Roles.Select(r => r.Nombre).ToList(),
            Permisos = usuario.Roles.SelectMany(r => r.Permisos).Select(p => p.Codigo).Distinct().ToList()
        };

        return Result.Success(dto);
    }
}
