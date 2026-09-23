using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public LogoutCommandHandler(
        ISesionRepository sesionRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _sesionRepository = sesionRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Failure(new Error("INVALID_TOKEN", "El token de refresco no es válido."));
        }

        var sesion = await _sesionRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (sesion != null && sesion.Activa)
        {
            sesion.Desactivar();
            _sesionRepository.Update(sesion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var usuario = await _usuarioRepository.GetByIdAsync(sesion.UsuarioId);

            // Auditar logout
            await _auditService.LogActionAsync(
                usuarioId: sesion.UsuarioId,
                usuarioNombre: usuario?.Nombre ?? "Desconocido",
                rol: usuario?.Roles?.FirstOrDefault()?.Nombre ?? "Ninguno",
                modulo: "Seguridad",
                entidad: "Sesiones",
                entidadId: (int)sesion.Id,
                accion: "Cierre de sesión",
                estadoAnterior: "Activa",
                estadoNuevo: "Inactiva",
                canal: "API",
                correlationId: null);
        }

        return Result.Success();
    }
}
