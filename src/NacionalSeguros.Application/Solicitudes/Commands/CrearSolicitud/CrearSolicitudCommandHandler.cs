using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.CrearSolicitud;

public class CrearSolicitudCommandHandler : IRequestHandler<CrearSolicitudCommand, Result<SolicitudResponseDto>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    public CrearSolicitudCommandHandler(
        ISolicitudRepository solicitudRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPublisher publisher)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<Result<SolicitudResponseDto>> Handle(CrearSolicitudCommand request, CancellationToken cancellationToken)
    {
        Usuario? solicitante = null;
        if (request.UsuarioId.HasValue)
        {
            solicitante = await _usuarioRepository.GetByIdAsync(request.UsuarioId.Value);
        }

        var createdByEmail = request.CreatedBy;
        if (createdByEmail == "Sistema" || string.IsNullOrWhiteSpace(createdByEmail))
        {
            createdByEmail = "system@nacionalseguros.com.bo";
        }

        if (solicitante == null)
        {
            solicitante = await _usuarioRepository.GetByCorreoAsync(createdByEmail);
        }

        if (solicitante == null)
        {
            solicitante = await _usuarioRepository.GetByCorreoAsync("admin@nacionalseguros.com.bo")
                          ?? await _usuarioRepository.GetByCorreoAsync("admin@nacionalseguros.com")
                          ?? await _usuarioRepository.GetByCorreoAsync("rrhh@nacionalseguros.com.bo");

            if (solicitante == null)
            {
                var allUsersResult = await _usuarioRepository.GetPagedAsync(1, 1, null);
                solicitante = allUsersResult.Items.FirstOrDefault();
            }

            if (solicitante == null)
            {
                return Result.Failure<SolicitudResponseDto>(new Error("Usuario.NotFound", "No se encontró ningún usuario solicitante ni alternativos."));
            }
        }

        var estadoRecibida = await _solicitudRepository.GetEstadoByCodigoAsync("SOL-REC");
        if (estadoRecibida == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Estado.NotFound", "El estado inicial de Solicitud 'SOL-REC' no existe."));
        }

        var dto = request.Dto;

        var solicitud = new Solicitud(
            cargo: dto.Cargo,
            solicitanteId: solicitante.Id,
            regionalId: dto.RegionalId,
            tipoSolicitudId: dto.TipoSolicitudId,
            modalidadTrabajoId: dto.ModalidadTrabajoId,
            seniority: dto.Seniority,
            prioridad: dto.Prioridad,
            funciones: dto.Funciones,
            estadoId: estadoRecibida.Id,
            objetivoCargo: dto.ObjetivoCargo,
            formacionAcademica: dto.FormacionAcademica,
            experienciaMinima: dto.ExperienciaMinima,
            experienciaIndispensable: dto.ExperienciaIndispensable,
            conocimientosTecnicos: dto.ConocimientosTecnicos,
            herramientasSistemas: dto.HerramientasSistemas,
            competenciasClave: dto.CompetenciasClave,
            disponibilidadRequerida: dto.DisponibilidadRequerida,
            criteriosExcluyentes: dto.CriteriosExcluyentes,
            criteriosDeseables: dto.CriteriosDeseables,
            motivo: dto.Motivo,
            cantidadVacantes: dto.CantidadVacantes,
            observaciones: dto.Observaciones,
            canalOrigen: request.CanalOrigen,
            workflowOrigen: request.WorkflowOrigen,
            apiKeyId: request.ApiKeyId,
            correlationId: request.CorrelationId,
            cargoId: dto.CargoId
        );

        solicitud.SetEstado(estadoRecibida);
        if (dto.CargoId.HasValue)
        {
            solicitud.SetCargoId(dto.CargoId.Value);
        }

        await _solicitudRepository.AddAsync(solicitud);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var estadoBorrador = await _solicitudRepository.GetEstadoByCodigoAsync("SOL-BOR");
        if (estadoBorrador == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Estado.NotFound", "El estado 'SOL-BOR' no existe."));
        }

        solicitud.Transitar(estadoBorrador, decisorId: null, solicitante.Correo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.WorkflowOrigen == "n8n_integration")
        {
            var estadoEnv = await _solicitudRepository.GetEstadoByCodigoAsync("SOL-ENV");
            if (estadoEnv == null)
            {
                return Result.Failure<SolicitudResponseDto>(new Error("Estado.NotFound", "El estado 'SOL-ENV' no existe."));
            }
            solicitud.Transitar(estadoEnv, decisorId: null, solicitante.Correo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Publicar el evento de creación de solicitud
        var createdEvent = new NacionalSeguros.Domain.Events.SolicitudCreadaEvent(
            solicitud.Id,
            solicitud.Cargo,
            solicitud.SolicitanteId
        );
        await _publisher.Publish(new Abstractions.Events.DomainEventNotification<NacionalSeguros.Domain.Events.SolicitudCreadaEvent>(createdEvent), cancellationToken);

        // Pre-cargar la relación para la respuesta
        var res = await _solicitudRepository.GetByIdAsync(solicitud.Id);
        
        var responseDto = _mapper.Map<SolicitudResponseDto>(res ?? solicitud);
        return Result.Success(responseDto);
    }
}
