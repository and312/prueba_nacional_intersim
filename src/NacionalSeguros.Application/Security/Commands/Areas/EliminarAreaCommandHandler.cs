using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Areas;

public class EliminarAreaCommandHandler : IRequestHandler<EliminarAreaCommand, Result>
{
    private readonly IAreaRepository _areaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public EliminarAreaCommandHandler(
        IAreaRepository areaRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _areaRepository = areaRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result> Handle(EliminarAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _areaRepository.GetByIdAsync(request.AreaId);
        if (area == null || area.IsDeleted)
        {
            return Result.Failure(new Error("AREA_NOT_FOUND", "El área no existe."));
        }

        // Regla de Negocio: No permitir eliminar un área con usuarios asociados
        bool tieneUsuarios = await _areaRepository.HasUsersAsync(request.AreaId);
        if (tieneUsuarios)
        {
            return Result.Failure(new Error("AREA_HAS_ASSOCIATED_USERS", "No se puede eliminar el área porque tiene usuarios asociados."));
        }

        area.EliminarLogicamente();
        _areaRepository.Update(area);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar eliminación
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Areas",
            entidadId: area.Id,
            accion: "Eliminar área",
            estadoAnterior: $"AreaId: {area.Id}, Código: {area.Codigo}, Nombre: {area.Nombre}",
            estadoNuevo: "Eliminado lógicamente",
            canal: "API",
            correlationId: null);

        return Result.Success();
    }
}
