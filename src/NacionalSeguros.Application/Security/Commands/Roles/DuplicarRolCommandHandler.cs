using System;
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

public class DuplicarRolCommandHandler : IRequestHandler<DuplicarRolCommand, Result<RolResponseDto>>
{
    private readonly IRolRepository _rolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public DuplicarRolCommandHandler(
        IRolRepository rolRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditService auditService)
    {
        _rolRepository = rolRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<Result<RolResponseDto>> Handle(DuplicarRolCommand request, CancellationToken cancellationToken)
    {
        var original = await _rolRepository.GetByIdAsync(request.RolId);
        if (original == null || original.IsDeleted)
        {
            return Result.Failure<RolResponseDto>(new Error("ROLE_NOT_FOUND", "El rol original no existe."));
        }

        var roles = await _rolRepository.ListAsync();
        if (roles.Any(r => r.Nombre.Equals(request.NuevoNombre, StringComparison.OrdinalIgnoreCase) && !r.IsDeleted))
        {
            return Result.Failure<RolResponseDto>(new Error("ROLE_NAME_ALREADY_EXISTS", "El nuevo nombre del rol ya está registrado."));
        }

        var duplicado = new Rol(request.NuevoNombre, $"Duplicado de: {original.Nombre}. {original.Descripcion}");

        // Copiar los permisos
        foreach (var permiso in original.Permisos)
        {
            duplicado.AsignarPermiso(permiso);
        }

        await _rolRepository.AddAsync(duplicado);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar duplicación
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Roles",
            entidadId: duplicado.Id,
            accion: "Duplicar rol",
            estadoAnterior: $"RolId Original: {original.Id}, Nombre Original: {original.Nombre}",
            estadoNuevo: $"RolId Nuevo: {duplicado.Id}, Nombre Nuevo: {duplicado.Nombre}",
            canal: "API",
            correlationId: null);

        var dto = _mapper.Map<RolResponseDto>(duplicado);
        return Result.Success(dto);
    }
}
