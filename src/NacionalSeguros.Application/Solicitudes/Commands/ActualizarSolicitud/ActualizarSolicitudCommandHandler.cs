using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.ActualizarSolicitud;

public class ActualizarSolicitudCommandHandler : IRequestHandler<ActualizarSolicitudCommand, Result<SolicitudResponseDto>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ActualizarSolicitudCommandHandler(
        ISolicitudRepository solicitudRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<SolicitudResponseDto>> Handle(ActualizarSolicitudCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByCorreoAsync(request.ModifiedBy);
        if (usuario == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Usuario.NotFound", $"El usuario modificado '{request.ModifiedBy}' no existe."));
        }

        var solicitud = await _solicitudRepository.GetByIdAsync(request.Id);
        if (solicitud == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Id} no existe."));
        }

        // Validación de Seguridad: Si el usuario es Solicitante, solo puede editar sus propias solicitudes
        bool esSolicitante = usuario.Roles.Any(r => r.Nombre.Equals("Solicitante", StringComparison.OrdinalIgnoreCase));
        if (esSolicitante && solicitud.SolicitanteId != usuario.Id)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.UnauthorizedEdit", "Un Solicitante solo puede editar las solicitudes creadas por él mismo."));
        }

        string? estadoAnteriorCodigo = solicitud.Estado?.Codigo;

        if (solicitud.Estado?.Codigo == "SOL-OBS")
        {
            var estadoCorregida = await _solicitudRepository.GetEstadoByCodigoAsync("SOL-COR");
            if (estadoCorregida != null)
            {
                solicitud.Transitar(estadoCorregida, decisorId: null, usuario.Correo);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var estadoBorrador = await _solicitudRepository.GetEstadoByCodigoAsync("SOL-BOR");
            if (estadoBorrador != null)
            {
                solicitud.Transitar(estadoBorrador, decisorId: null, usuario.Correo);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        try
        {
            var dto = request.Dto;
            solicitud.Actualizar(
                cargo: dto.Cargo,
                regionalId: dto.RegionalId,
                tipoSolicitudId: dto.TipoSolicitudId,
                modalidadTrabajoId: dto.ModalidadTrabajoId,
                seniority: dto.Seniority,
                prioridad: dto.Prioridad,
                funciones: dto.Funciones,
                modificadoPor: usuario.Correo,
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
                estadoAnteriorCodigoOverride: estadoAnteriorCodigo
            );

            _solicitudRepository.Update(solicitud);

            if (!string.IsNullOrWhiteSpace(dto.Observaciones))
            {
                solicitud.AgregarComentario(
                    usuarioId: usuario.Id,
                    texto: dto.Observaciones.Trim(),
                    estadoAsociado: "SOL-ENV",
                    iteracion: solicitud.Iteracion,
                    tipoComentario: "RESPUESTA_SOLICITANTE"
                );
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var res = await _solicitudRepository.GetByIdAsync(solicitud.Id);

            var responseDto = _mapper.Map<SolicitudResponseDto>(res ?? solicitud);
            return Result.Success(responseDto);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("INVALID_STATE_TRANSITION", ex.Message));
        }
    }
}
