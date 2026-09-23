using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.CrearVacante;

public class CrearVacanteCommandHandler : IRequestHandler<CrearVacanteCommand, Result<VacanteResponseDto>>
{
    private readonly IVacanteRepository _vacanteRepository;
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CrearVacanteCommandHandler(
        IVacanteRepository vacanteRepository,
        ISolicitudRepository solicitudRepository,
        IPerfilCargoRepository perfilCargoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<VacanteResponseDto>> Handle(CrearVacanteCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(request.Dto.SolicitudId);
        if (solicitud == null)
        {
            return Result.Failure<VacanteResponseDto>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Dto.SolicitudId} no existe."));
        }

        var perfil = await _perfilCargoRepository.GetByIdAsync(request.Dto.PerfilId);
        if (perfil == null)
        {
            return Result.Failure<VacanteResponseDto>(new Error("Perfil.NotFound", $"El perfil de cargo con ID {request.Dto.PerfilId} no existe."));
        }

        // Regla: No se puede crear una vacante si el perfil no está aprobado
        // Nota: El perfil de cargo tiene EstadoId = 3 para Aprobada (SOL-APR)
        if (perfil.EstadoId != 3)
        {
            return Result.Failure<VacanteResponseDto>(new Error("Perfil.NotApproved", "No se puede abrir una vacante si el perfil de cargo no se encuentra en estado Aprobado."));
        }

        var estadoInicial = await _vacanteRepository.GetEstadoByCodigoAsync("VAC-CRE");
        if (estadoInicial == null)
        {
            return Result.Failure<VacanteResponseDto>(new Error("Estado.NotFound", "El estado inicial 'VAC-CRE' (Creada) no está parametrizado."));
        }

        // La solicitud no almacena la remuneración, por lo que se establecen valores de banda por defecto
        decimal bandaMin = 5000;
        decimal bandaMax = 10000;

        var vacante = new Vacante(
            perfilCargoId: request.Dto.PerfilId,
            solicitudId: request.Dto.SolicitudId,
            estadoId: estadoInicial.Id,
            bandaSalarialMin: bandaMin,
            bandaSalarialMax: bandaMax,
            createdBy: request.CreatedBy
        );

        await _vacanteRepository.AddAsync(vacante);
        
        // Registrar comentario en la base de datos a través de UnitOfWork
        _unitOfWork.TransitionComment = "Creacion de la vacante a partir de solicitud y perfil aprobados.";

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Pre-cargar relaciones para mapear al DTO
        var res = await _vacanteRepository.GetByIdAsync(vacante.Id);

        var responseDto = _mapper.Map<VacanteResponseDto>(res ?? vacante);
        return Result.Success(responseDto);
    }
}
