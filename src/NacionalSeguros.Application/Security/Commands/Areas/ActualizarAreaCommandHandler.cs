using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Areas;

public class ActualizarAreaCommandHandler : IRequestHandler<ActualizarAreaCommand, Result<AreaResponseDto>>
{
    private readonly IAreaRepository _areaRepository;
    private readonly IGerenciaRepository _gerenciaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public ActualizarAreaCommandHandler(
        IAreaRepository areaRepository,
        IGerenciaRepository gerenciaRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditService auditService)
    {
        _areaRepository = areaRepository;
        _gerenciaRepository = gerenciaRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<Result<AreaResponseDto>> Handle(ActualizarAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _areaRepository.GetByIdAsync(request.AreaId);
        if (area == null || area.IsDeleted)
        {
            return Result.Failure<AreaResponseDto>(new Error("AREA_NOT_FOUND", "El área no existe."));
        }

        // Verificar si la gerencia existe en el sistema
        var gerencia = await _gerenciaRepository.GetByIdAsync(request.GerenciaId);
        if (gerencia == null)
        {
            return Result.Failure<AreaResponseDto>(new Error("GERENCIA_NOT_FOUND", "La gerencia seleccionada no existe en el sistema."));
        }

        string nombreAnterior = area.Nombre;
        string estadoAnterior = area.Estado;
        int gerenciaIdAnterior = area.GerenciaId;

        area.Actualizar(request.Nombre, request.GerenciaId, request.Responsable, "Admin");

        if (request.Estado.Equals("Activo", System.StringComparison.OrdinalIgnoreCase))
        {
            area.Activar("Admin");
        }
        else
        {
            area.Desactivar("Admin");
        }

        _areaRepository.Update(area);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Volver a cargar para incluir la propiedad de navegación Gerencia al mapear
        var areaCargada = await _areaRepository.GetByIdAsync(area.Id);

        // Auditar actualización
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Areas",
            entidadId: area.Id,
            accion: "Actualizar área",
            estadoAnterior: $"Nombre: {nombreAnterior}, Estado: {estadoAnterior}, GerenciaId: {gerenciaIdAnterior}",
            estadoNuevo: $"Nombre: {area.Nombre}, Estado: {area.Estado}, GerenciaId: {area.GerenciaId}",
            canal: "API",
            correlationId: null);

        var dto = _mapper.Map<AreaResponseDto>(areaCargada ?? area);
        return Result.Success(dto);
    }
}
