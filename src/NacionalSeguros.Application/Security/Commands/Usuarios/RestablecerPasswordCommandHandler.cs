using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public class RestablecerPasswordCommandHandler : IRequestHandler<RestablecerPasswordCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public RestablecerPasswordCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result> Handle(RestablecerPasswordCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);
        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure(new Error("USER_NOT_FOUND", "El usuario no existe."));
        }

        if (string.IsNullOrWhiteSpace(request.NuevaClave))
        {
            return Result.Failure(new Error("PASSWORD_REQUIRED", "La nueva contraseña es requerida."));
        }

        string claveHash = _passwordHasher.HashPassword(request.NuevaClave);
        usuario.CambiarPassword(claveHash, "Admin");

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar cambio de contraseña
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Usuarios",
            entidadId: usuario.Id,
            accion: "Restablecer contraseña",
            estadoAnterior: "Contraseña anterior cifrada",
            estadoNuevo: "Nueva contraseña establecida",
            canal: "API",
            correlationId: null);

        return Result.Success();
    }
}
