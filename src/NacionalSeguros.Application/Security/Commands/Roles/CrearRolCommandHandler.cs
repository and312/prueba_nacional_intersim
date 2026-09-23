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

public class CrearRolCommandHandler : IRequestHandler<CrearRolCommand, Result<RolResponseDto>>
{
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public CrearRolCommandHandler(
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

    public async Task<Result<RolResponseDto>> Handle(CrearRolCommand request, CancellationToken cancellationToken)
    {
        var roles = await _rolRepository.ListAsync();
        if (roles.Any(r => r.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) && !r.IsDeleted))
        {
            return Result.Failure<RolResponseDto>(new Error("ROLE_ALREADY_EXISTS", "El nombre de rol ya está registrado."));
        }

        var rol = new Rol(request.Nombre, request.Descripcion);

        if (request.PermisoCodigos != null && request.PermisoCodigos.Any())
        {
            var todosPermisos = await _permisoRepository.ListAsync();
            var permisosAsignar = todosPermisos.Where(p => request.PermisoCodigos.Contains(p.Codigo));
            foreach (var permiso in permisosAsignar)
            {
                rol.AsignarPermiso(permiso);
            }
        }

        await _rolRepository.AddAsync(rol);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar creación de rol
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Roles",
            entidadId: rol.Id,
            accion: "Crear rol",
            estadoAnterior: null,
            estadoNuevo: $"RolId: {rol.Id}, Nombre: {rol.Nombre}",
            canal: "API",
            correlationId: null);

        var dto = _mapper.Map<RolResponseDto>(rol);
        return Result.Success(dto);
    }
}
