using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Areas;

public class CrearAreaCommandHandler : IRequestHandler<CrearAreaCommand, Result<AreaResponseDto>>
{
    private readonly IAreaRepository _areaRepository;
    private readonly IGerenciaRepository _gerenciaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public CrearAreaCommandHandler(
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

    public async Task<Result<AreaResponseDto>> Handle(CrearAreaCommand request, CancellationToken cancellationToken)
    {
        var existente = await _areaRepository.GetByCodigoAsync(request.Codigo);
        if (existente != null)
        {
            return Result.Failure<AreaResponseDto>(new Error("AREA_CODE_ALREADY_EXISTS", "El código de área ya está registrado."));
        }

        // Verificar si la gerencia existe en el sistema
        var gerencia = await _gerenciaRepository.GetByIdAsync(request.GerenciaId);
        if (gerencia == null)
        {
            return Result.Failure<AreaResponseDto>(new Error("GERENCIA_NOT_FOUND", "La gerencia seleccionada no existe en el sistema."));
        }

        var area = new Area(request.Codigo, request.Nombre, request.GerenciaId, request.Responsable);

        await _areaRepository.AddAsync(area);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Volver a cargar para incluir la propiedad de navegación Gerencia al mapear
        var areaCargada = await _areaRepository.GetByIdAsync(area.Id);

        // Auditar creación
        await _auditService.LogActionAsync(
            usuarioId: null,
            usuarioNombre: "Admin",
            rol: "Administrador",
            modulo: "Seguridad",
            entidad: "Areas",
            entidadId: area.Id,
            accion: "Crear área",
            estadoAnterior: null,
            estadoNuevo: $"AreaId: {area.Id}, Código: {area.Codigo}, Nombre: {area.Nombre}, GerenciaId: {area.GerenciaId}",
            canal: "API",
            correlationId: null);

        var dto = _mapper.Map<AreaResponseDto>(areaCargada ?? area);
        return Result.Success(dto);
    }
}
