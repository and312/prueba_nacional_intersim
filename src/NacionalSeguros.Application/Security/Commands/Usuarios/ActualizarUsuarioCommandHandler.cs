using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public class ActualizarUsuarioCommandHandler : IRequestHandler<ActualizarUsuarioCommand, Result<UsuarioResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public ActualizarUsuarioCommandHandler(
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

    public async Task<Result<UsuarioResponseDto>> Handle(ActualizarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);

        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure<UsuarioResponseDto>(new Error("USER_NOT_FOUND", "El usuario no existe."));
        }

        string nombreAnterior = usuario.Nombre;
        string estadoAnterior = usuario.Estado.ToString();

        // Validar y actualizar correo si ha cambiado
        var correoNormalizado = request.Correo?.Trim() ?? string.Empty;
        if (!string.Equals(usuario.Correo, correoNormalizado, System.StringComparison.OrdinalIgnoreCase))
        {
            var existente = await _usuarioRepository.GetByCorreoAsync(correoNormalizado);
            if (existente != null)
            {
                return Result.Failure<UsuarioResponseDto>(new Error("USER_ALREADY_EXISTS", "El correo electrónico ya está registrado por otro usuario."));
            }
        }

        // Actualizar datos personales y organizacionales
        usuario.ActualizarDatosCompletos(
            correoNormalizado,
            request.Nombres,
            request.Apellidos,
            request.AreaId,
            request.Cargo,
            request.Gerencia,
            request.Telefono,
            request.Extension,
            request.Observaciones,
            request.FotografiaUrl,
            "Admin");

        // Actualizar estado
        if (System.Enum.TryParse<UsuarioEstado>(request.Estado, true, out var nuevoEstado))
        {
            if (nuevoEstado == UsuarioEstado.Activo) usuario.Activar("Admin");
            else if (nuevoEstado == UsuarioEstado.Inactivo) usuario.Inactivar("Admin");
            else if (nuevoEstado == UsuarioEstado.Bloqueado) usuario.Bloquear("Admin");
        }

        // Actualizar roles (máximo 1 rol)
        usuario.LimpiarRoles();
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

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar actualización
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Usuarios",
            entidadId: usuario.Id,
            accion: "Actualizar usuario",
            estadoAnterior: $"Nombre: {nombreAnterior}, Estado: {estadoAnterior}",
            estadoNuevo: $"Nombre: {usuario.Nombre}, Estado: {usuario.Estado}, AreaId: {usuario.AreaId}",
            canal: "API",
            correlationId: null);

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
