using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Roles;

public class ActualizarRolCommandHandler : IRequestHandler<ActualizarRolCommand, Result<RolResponseDto>>
{
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public ActualizarRolCommandHandler(
        IRolRepository rolRepository,
        IPermisoRepository permisoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditService auditService)
    {
        _rolRepository = rolRepository;
        _permisoRepository = permisoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<Result<RolResponseDto>> Handle(ActualizarRolCommand request, CancellationToken cancellationToken)
    {
        var rol = await _rolRepository.GetByIdAsync(request.RolId);
        if (rol == null || rol.IsDeleted)
        {
            return Result.Failure<RolResponseDto>(new Error("ROLE_NOT_FOUND", "El rol no existe."));
        }

        var roles = await _rolRepository.ListAsync();
        if (roles.Any(r => r.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) && r.Id != request.RolId && !r.IsDeleted))
        {
            return Result.Failure<RolResponseDto>(new Error("ROLE_NAME_ALREADY_EXISTS", "El nombre del rol ya está siendo utilizado por otro rol."));
        }

        string nombreAnterior = rol.Nombre;
        string? descAnterior = rol.Descripcion;

        rol.Actualizar(request.Nombre, request.Descripcion);

        // Actualizar permisos
        rol.LimpiarPermisos();
        if (request.PermisoCodigos != null && request.PermisoCodigos.Any())
        {
            var todosPermisos = await _permisoRepository.ListAsync();
            var permisosAsignar = todosPermisos.Where(p => request.PermisoCodigos.Contains(p.Codigo));
            foreach (var permiso in permisosAsignar)
            {
                rol.AsignarPermiso(permiso);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar actualización de rol
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Roles",
            entidadId: rol.Id,
            accion: "Actualizar rol",
            estadoAnterior: $"Nombre: {nombreAnterior}, Desc: {descAnterior}",
            estadoNuevo: $"Nombre: {rol.Nombre}, Desc: {rol.Descripcion}",
            canal: "API",
            correlationId: null);

        var dto = _mapper.Map<RolResponseDto>(rol);
        return Result.Success(dto);
    }
}
