using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public class AsignarRolesUsuarioCommandHandler : IRequestHandler<AsignarRolesUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public AsignarRolesUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result> Handle(AsignarRolesUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);

        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure(new Error("USER_NOT_FOUND", "El usuario no existe."));
        }

        string rolesAnteriores = string.Join(", ", usuario.Roles.Select(r => r.Nombre));

        usuario.LimpiarRoles();

        foreach (int rolId in request.RolIds)
        {
            var rol = await _rolRepository.GetByIdAsync(rolId);
            if (rol != null && !rol.IsDeleted)
            {
                usuario.AsignarRol(rol);
            }
            else
            {
                return Result.Failure(new Error("ROLE_NOT_FOUND", $"El rol con ID {rolId} no existe o está eliminado."));
            }
        }

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string rolesNuevos = string.Join(", ", usuario.Roles.Select(r => r.Nombre));

        // Auditar cambio de roles
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Usuarios",
            entidadId: usuario.Id,
            accion: "Asignar roles (RBAC)",
            estadoAnterior: $"Roles: {rolesAnteriores}",
            estadoNuevo: $"Roles: {rolesNuevos}",
            canal: "API",
            correlationId: null);

        return Result.Success();
    }
}
