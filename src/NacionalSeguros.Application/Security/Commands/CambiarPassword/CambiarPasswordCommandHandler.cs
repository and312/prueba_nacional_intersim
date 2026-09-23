using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.CambiarPassword;

public class CambiarPasswordCommandHandler : IRequestHandler<CambiarPasswordCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public CambiarPasswordCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
    }

    public async Task<Result> Handle(CambiarPasswordCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);

        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure(new Error("USER_NOT_FOUND", "El usuario no existe."));
        }

        if (usuario.TipoAutenticacion == TipoAutenticacion.ActiveDirectory)
        {
            return Result.Failure(new Error("METHOD_NOT_ALLOWED", "El cambio de contraseña está deshabilitado para usuarios de Active Directory."));
        }

        if (string.IsNullOrEmpty(usuario.ClaveHash) || !_passwordHasher.VerifyPassword(request.ClaveActual, usuario.ClaveHash))
        {
            return Result.Failure(new Error("INVALID_CURRENT_PASSWORD", "La contraseña actual es incorrecta."));
        }

        string nuevoHash = _passwordHasher.HashPassword(request.NuevaClave);
        usuario.CambiarPassword(nuevoHash, usuario.Nombre);

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar el cambio de contraseña
        await _auditService.LogActionAsync(
            usuarioId: usuario.Id,
            usuarioNombre: usuario.Nombre,
            rol: usuario.Roles.FirstOrDefault()?.Nombre ?? "Ninguno",
            modulo: "Seguridad",
            entidad: "Usuarios",
            entidadId: usuario.Id,
            accion: "Cambio de contraseña",
            estadoAnterior: "Clave antigua activa",
            estadoNuevo: "Clave nueva establecida",
            canal: "API",
            correlationId: null);

        return Result.Success();
    }
}
