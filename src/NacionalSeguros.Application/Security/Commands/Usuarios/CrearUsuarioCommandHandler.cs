using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public class CrearUsuarioCommandHandler : IRequestHandler<CrearUsuarioCommand, Result<UsuarioResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public CrearUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
    }

    public async Task<Result<UsuarioResponseDto>> Handle(CrearUsuarioCommand request, CancellationToken cancellationToken)
    {
        var correoNormalizado = request.Correo?.Trim() ?? string.Empty;
        var nombresNormalizados = request.Nombres?.Trim() ?? string.Empty;
        var apellidosNormalizados = request.Apellidos?.Trim() ?? string.Empty;

        var existente = await _usuarioRepository.GetByCorreoAsync(correoNormalizado);
        if (existente != null)
        {
            return Result.Failure<UsuarioResponseDto>(new Error("USER_ALREADY_EXISTS", "El correo electrónico ya está registrado."));
        }

        var tipoAuth = Enum.TryParse<TipoAutenticacion>(request.TipoAutenticacion, out var resultType)
            ? resultType
            : TipoAutenticacion.Local;

        // Validar la regla de clave AD vs Local (CWE-256 / CK_Usuarios_ClaveHash_AD)
        if (tipoAuth == TipoAutenticacion.Local && string.IsNullOrWhiteSpace(request.Clave))
        {
            return Result.Failure<UsuarioResponseDto>(new Error("PASSWORD_REQUIRED", "La contraseña es requerida para autenticación local."));
        }

        var usuario = new Usuario(
            $"{nombresNormalizados} {apellidosNormalizados}".Trim(),
            correoNormalizado,
            tipoAuth,
            request.AreaId,
            nombresNormalizados,
            apellidosNormalizados,
            request.Cargo,
            request.Gerencia,
            request.Telefono,
            request.Extension,
            request.Observaciones,
            request.FotografiaUrl,
            tipoAuth == TipoAutenticacion.ActiveDirectory ? correoNormalizado : null);

        if (tipoAuth == TipoAutenticacion.Local)
        {
            string claveHash = _passwordHasher.HashPassword(request.Clave!);
            usuario.CambiarPassword(claveHash, "Admin");
        }

        // Asignar roles si se especificaron (máximo 1 rol)
        if (request.RolIds != null && request.RolIds.Any())
        {
            foreach (int rolId in request.RolIds.Take(1))
            {
                var rol = await _rolRepository.GetByIdAsync(rolId);
                if (rol != null && !rol.IsDeleted)
                {
                    usuario.AsignarRol(rol);
                }
            }
        }

        await _usuarioRepository.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar creación
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Usuarios",
            entidadId: usuario.Id,
            accion: "Crear usuario",
            estadoAnterior: null,
            estadoNuevo: $"UsuarioId: {usuario.Id}, Correo: {usuario.Correo}, AreaId: {usuario.AreaId}",
            canal: "API",
            correlationId: null);

        // Recargar el usuario con su Área para mapear correctamente
        var usuarioCargado = await _usuarioRepository.GetByIdAsync(usuario.Id);

        var dto = new UsuarioResponseDto
        {
            UsuarioId = usuario.Id,
            Correo = usuario.Correo,
            Nombres = usuario.Nombres ?? usuario.Nombre,
            Apellidos = usuario.Apellidos ?? string.Empty,
            TipoAutenticacion = usuario.TipoAutenticacion.ToString(),
            Activo = usuario.Estado == UsuarioEstado.Activo,
            Estado = usuario.Estado.ToString(),
            AreaId = usuario.AreaId,
            AreaNombre = usuarioCargado?.Area?.Nombre ?? string.Empty,
            Cargo = usuario.Cargo,
            Gerencia = usuario.Gerencia,
            Telefono = usuario.Telefono,
            Extension = usuario.Extension,
            Observaciones = usuario.Observaciones,
            FotografiaUrl = usuario.FotografiaUrl,
            FechaCreacion = usuario.CreatedDate,
            Roles = usuario.Roles.Select(r => r.Nombre).ToList(),
            Permisos = usuario.Roles.SelectMany(r => r.Permisos).Select(p => p.Codigo).Distinct().ToList()
        };

        return Result.Success(dto);
    }
}
