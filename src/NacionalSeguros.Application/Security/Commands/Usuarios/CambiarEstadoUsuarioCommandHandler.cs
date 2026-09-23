using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public class CambiarEstadoUsuarioCommandHandler : IRequestHandler<CambiarEstadoUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CambiarEstadoUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result> Handle(CambiarEstadoUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);
        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure(new Error("USER_NOT_FOUND", "El usuario no existe."));
        }

        string estadoAnterior = usuario.Estado.ToString();

        if (!System.Enum.TryParse<UsuarioEstado>(request.NuevoEstado, true, out var nuevoEstado))
        {
            return Result.Failure(new Error("INVALID_STATUS", "El estado especificado no es válido."));
        }

        if (nuevoEstado == UsuarioEstado.Activo)
        {
            usuario.Activar("Admin");
        }
        else if (nuevoEstado == UsuarioEstado.Inactivo)
        {
            usuario.Inactivar("Admin");
        }
        else if (nuevoEstado == UsuarioEstado.Bloqueado)
        {
            usuario.Bloquear("Admin");
        }

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar cambio de estado
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Usuarios",
            entidadId: usuario.Id,
            accion: "Cambiar estado usuario",
            estadoAnterior: $"Estado: {estadoAnterior}",
            estadoNuevo: $"Estado: {usuario.Estado}",
            canal: "API",
            correlationId: null);

        return Result.Success();
    }
}
