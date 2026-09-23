using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.EnviarSolicitudARRHH;

public class EnviarSolicitudARRHHCommandHandler : IRequestHandler<EnviarSolicitudARRHHCommand, Result<SolicitudResponseDto>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EnviarSolicitudARRHHCommandHandler(
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

    public async Task<Result<SolicitudResponseDto>> Handle(EnviarSolicitudARRHHCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByCorreoAsync(request.UserEmail);
        if (usuario == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Usuario.NotFound", $"El usuario '{request.UserEmail}' no existe."));
        }

        var solicitud = await _solicitudRepository.GetByIdAsync(request.Id);
        if (solicitud == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Id} no existe."));
        }

        // Validación de Seguridad: Si el usuario es Solicitante, solo puede enviar sus propias solicitudes
        bool esSolicitante = usuario.Roles.Any(r => r.Nombre.Equals("Solicitante", StringComparison.OrdinalIgnoreCase));
        if (esSolicitante && solicitud.SolicitanteId != usuario.Id)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.UnauthorizedSend", "Un Solicitante solo puede enviar las solicitudes creadas por él mismo."));
        }

        // Validar Estado de la Solicitud
        if (solicitud.Estado?.Codigo != "SOL-REG" && solicitud.Estado?.Codigo != "SOL-BOR" && solicitud.Estado?.Codigo != "SOL-PEN" && solicitud.Estado?.Codigo != "SOL-OBS")
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.InvalidStateForSend", "Solo se pueden enviar solicitudes en estado Borrador, Registrada, Pendiente u Observada."));
        }

        // Validaciones del PRD (Campos obligatorios)
        if (string.IsNullOrWhiteSpace(solicitud.Cargo))
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "El cargo es obligatorio."));
        }
        if (!solicitud.RegionalId.HasValue)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "La regional es obligatoria."));
        }
        if (!solicitud.CantidadVacantes.HasValue || solicitud.CantidadVacantes.Value <= 0)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "La cantidad de vacantes debe ser mayor a 0."));
        }
        if (!solicitud.TipoSolicitudId.HasValue)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "El tipo de solicitud es obligatorio."));
        }
        if (string.IsNullOrWhiteSpace(solicitud.Motivo))
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "El motivo de la vacante es obligatorio."));
        }
        if (!solicitud.ModalidadTrabajoId.HasValue)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "La modalidad de trabajo es obligatoria."));
        }
        if (string.IsNullOrWhiteSpace(solicitud.Seniority))
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "El seniority del cargo es obligatorio."));
        }
        if (string.IsNullOrWhiteSpace(solicitud.Prioridad))
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "La prioridad es obligatoria."));
        }
        if (string.IsNullOrWhiteSpace(solicitud.ObjetivoCargo))
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "El objetivo principal del cargo es obligatorio."));
        }
        if (string.IsNullOrWhiteSpace(solicitud.ExperienciaMinima))
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "La experiencia mínima requerida es obligatoria."));
        }
        if (string.IsNullOrWhiteSpace(solicitud.ConocimientosTecnicos))
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "Los conocimientos técnicos son obligatorios."));
        }
        if (string.IsNullOrWhiteSpace(solicitud.Funciones))
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Validation", "Las funciones del cargo son obligatorias."));
        }

        var estadoEnv = await _solicitudRepository.GetEstadoByCodigoAsync("SOL-ENV");
        if (estadoEnv == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Estado.NotFound", "El estado con código 'SOL-ENV' no existe en el catálogo."));
        }

        try
        {
            solicitud.Transitar(estadoEnv, decisorId: null, usuario.Correo);
            _unitOfWork.TransitionComment = "Solicitud enviada a revisión de RRHH.";
            _solicitudRepository.Update(solicitud);
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
