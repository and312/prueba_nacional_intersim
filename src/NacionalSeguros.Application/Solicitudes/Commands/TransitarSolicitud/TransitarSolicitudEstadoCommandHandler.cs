using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.TransitarSolicitud;

public class TransitarSolicitudEstadoCommandHandler : IRequestHandler<TransitarSolicitudEstadoCommand, Result<SolicitudResponseDto>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TransitarSolicitudEstadoCommandHandler> _logger;

    public TransitarSolicitudEstadoCommandHandler(
        ISolicitudRepository solicitudRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TransitarSolicitudEstadoCommandHandler> logger)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SolicitudResponseDto>> Handle(TransitarSolicitudEstadoCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByCorreoAsync(request.ModifiedBy);
        if (usuario == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Usuario.NotFound", $"El usuario '{request.ModifiedBy}' no existe."));
        }

        var solicitud = await _solicitudRepository.GetByIdAsync(request.Id);
        if (solicitud == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Id} no existe."));
        }

        var nuevoEstado = await _solicitudRepository.GetEstadoByCodigoAsync(request.NuevoEstadoCodigo);
        if (nuevoEstado == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Estado.NotFound", $"El estado con código '{request.NuevoEstadoCodigo}' no existe en el catálogo."));
        }

        try
        {
            if (solicitud.Estado?.Codigo == "SOL-OBS" && nuevoEstado.Codigo == "SOL-ENV")
            {
                var estadoCorregida = await _solicitudRepository.GetEstadoByCodigoAsync("SOL-COR");
                if (estadoCorregida != null)
                {
                    solicitud.Transitar(estadoCorregida, decisorId: null, usuario.Correo);
                    _unitOfWork.TransitionComment = "Corrección de observaciones registrada";
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Comentario))
            {
                string? estadoAsociado = nuevoEstado.Codigo;
                int? iteracion = null;
                string? tipoComentario = "COMENTARIO";

                if (nuevoEstado.Codigo == "SOL-OBS")
                {
                    tipoComentario = "OBSERVACION";
                    iteracion = System.Linq.Enumerable.Count(solicitud.Comentarios, 
                        c => c.EstadoAsociado == "SOL-OBS" || c.Texto.Contains("SOL-OBS") || c.Texto.Contains("Observado")) + 1;
                }
                else if (nuevoEstado.Codigo == "SOL-RECH")
                {
                    tipoComentario = "RECHAZO";
                }
                else if (nuevoEstado.Codigo == "SOL-APR")
                {
                    tipoComentario = "APROBACION";
                }

                solicitud.AgregarComentario(usuario.Id, request.Comentario, estadoAsociado, iteracion, tipoComentario);
            }

            int? decisorId = request.DecisorId;
            if (nuevoEstado.Codigo == "SOL-APR" || nuevoEstado.Codigo == "SOL-RECH")
            {
                // Si el decisor no está explícitamente definido en la petición, usar el ID del usuario actual si tiene permisos
                decisorId ??= usuario.Id;
            }

            solicitud.Transitar(nuevoEstado, decisorId, usuario.Correo);

            if (nuevoEstado.Codigo == "SOL-OBS")
            {
                var fechaRef = await _solicitudRepository.GetFechaUltimaInteraccionSolicitanteAsync(solicitud.Id);
                var refDate = fechaRef ?? solicitud.CreatedDate;
                var horasTranscurridas = (DateTime.UtcNow - refDate).TotalHours;
                if (horasTranscurridas < 0) horasTranscurridas = 0;

                bool seMostroAdvertencia = request.SeMostroAdvertencia ?? (horasTranscurridas > 24.0);
                bool usuarioConfirmoEnvio = request.UsuarioConfirmoEnvio ?? (horasTranscurridas > 24.0);
                bool mensajeEnviado = true;

                if (horasTranscurridas > 24.0 && !usuarioConfirmoEnvio)
                {
                    _unitOfWork.SkipWhatsAppNotification = true;
                    mensajeEnviado = false;
                }

                string decisionTexto = (horasTranscurridas <= 24.0) ? "N/A (Automático)" : (usuarioConfirmoEnvio ? "Enviar WhatsApp" : "No enviar WhatsApp");
                string resultadoTexto = mensajeEnviado ? "Sí" : "Omitido por decisión del usuario";

                _logger.LogInformation(
                    "[Auditoría Observación WhatsApp] Fecha/Hora: {FechaHora}, SolicitudId: {SolicitudId}, Usuario RRHH: {Usuario}, Horas Transcurridas: {Horas:F2}, Se Mostró Advertencia: {Advertencia}, Decisión del usuario: {Decision}, Resultado del envío: {Resultado}",
                    DateTime.UtcNow, solicitud.Id, usuario.Correo, horasTranscurridas, seMostroAdvertencia ? "Sí" : "No", decisionTexto, resultadoTexto);
            }

            _unitOfWork.TransitionComment = request.Comentario ?? $"Transición de estado a {nuevoEstado.Nombre}";
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
