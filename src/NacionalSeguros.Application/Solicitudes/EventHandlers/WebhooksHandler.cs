using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NacionalSeguros.Domain.Events;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Application.Abstractions.Events;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.EventHandlers;

public class WebhooksHandler : 
    INotificationHandler<DomainEventNotification<SolicitudCreadaEvent>>,
    INotificationHandler<DomainEventNotification<SolicitudEstadoTransitadoEvent>>,
    INotificationHandler<DomainEventNotification<SolicitudActualizadaEvent>>
{
    private static readonly HttpClient HttpClient = new();
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebhooksHandler> _logger;

    public WebhooksHandler(
        ISolicitudRepository solicitudRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        ILogger<WebhooksHandler> logger)
    {
        _solicitudRepository = solicitudRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<SolicitudCreadaEvent> notification, CancellationToken cancellationToken)
    {
        var ev = notification.DomainEvent;
        _logger.LogInformation("Webhook: Procesando creación de solicitud {SolicitudId}...", ev.SolicitudId);

        var solicitud = await _solicitudRepository.GetByIdAsync(ev.SolicitudId);
        if (solicitud == null)
        {
            _logger.LogWarning("Webhook: No se encontró la solicitud {SolicitudId} para enviar el webhook de creación.", ev.SolicitudId);
            return;
        }

        if (solicitud.Estado?.Codigo == "SOL-BOR" || solicitud.Estado?.Codigo == "SOL-REG" || solicitud.Estado?.Codigo == "SOL-REC" || solicitud.Estado?.Codigo == "SOL-ENV")
        {
            string url = WebhookSettings.AresResumidorUrl;
            string correlationId = $"40000000-0000-0000-0000-{solicitud.Id:D12}";
            bool isWhatsApp = string.Equals(solicitud.CanalOrigen, "WHATSAPP", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(solicitud.CanalOrigen, "N8N", StringComparison.OrdinalIgnoreCase);
            string origen = isWhatsApp ? NacionalSeguros.Domain.Enums.WebhookOrigins.WhatsappCreation : NacionalSeguros.Domain.Enums.WebhookOrigins.BackofficeCreation;

            var payload = new
            {
                solicitudId = solicitud.Id,
                origen = origen,
                correlacionId = correlationId
            };

            // Disparar de forma asíncrona fire-and-forget
            _ = EnviarWebhookAsync(url, payload, solicitud.Id, origen, correlationId, "Creación");
        }
        else
        {
            _logger.LogInformation("Webhook: Omitiendo envío para solicitud {SolicitudId} ya que el estado es {EstadoCodigo}.", ev.SolicitudId, solicitud.Estado?.Codigo);
        }
    }

    public async Task Handle(DomainEventNotification<SolicitudActualizadaEvent> notification, CancellationToken cancellationToken)
    {
        var ev = notification.DomainEvent;
        string tipoEvento = ev.EstadoAnteriorCodigo == "SOL-OBS" ? "Corrección" : "Edición";
        _logger.LogInformation("Webhook: Procesando {TipoEvento} de solicitud {SolicitudId}...", tipoEvento, ev.SolicitudId);

        var solicitud = await _solicitudRepository.GetByIdAsync(ev.SolicitudId);
        if (solicitud == null)
        {
            _logger.LogWarning("Webhook: No se encontró la solicitud {SolicitudId} para enviar el webhook de {TipoEvento}.", ev.SolicitudId, tipoEvento);
            return;
        }

        string url = WebhookSettings.AresResumidorUrl;
        string correlationId = $"40000000-0000-0000-0000-{solicitud.Id:D12}";
        bool isWhatsApp = string.Equals(ev.Canal, "WHATSAPP", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(ev.Canal, "N8N", StringComparison.OrdinalIgnoreCase);
        string origen = isWhatsApp ? NacionalSeguros.Domain.Enums.WebhookOrigins.WhatsappUpdate : NacionalSeguros.Domain.Enums.WebhookOrigins.BackofficeUpdate;

        var payload = new
        {
            solicitudId = solicitud.Id,
            origen = origen,
            correlacionId = correlationId
        };

        // Disparar de forma asíncrona fire-and-forget
        _ = EnviarWebhookAsync(url, payload, solicitud.Id, origen, correlationId, tipoEvento);
    }

    public async Task Handle(DomainEventNotification<SolicitudEstadoTransitadoEvent> notification, CancellationToken cancellationToken)
    {
        var ev = notification.DomainEvent;
        
        if (ev.EstadoAnteriorId == ev.EstadoNuevoId)
        {
            _logger.LogInformation("Webhook: Omitiendo envío para solicitud {SolicitudId} porque el estado no ha cambiado ({EstadoId}).", ev.SolicitudId, ev.EstadoNuevoId);
            return;
        }

        _logger.LogInformation("Webhook: Procesando transición de solicitud {SolicitudId} al estado {EstadoNuevoId}...", ev.SolicitudId, ev.EstadoNuevoId);

        var solicitud = await _solicitudRepository.GetByIdAsync(ev.SolicitudId);
        if (solicitud == null)
        {
            _logger.LogWarning("Webhook: No se encontró la solicitud {SolicitudId} para evaluar webhooks de transición.", ev.SolicitudId);
            return;
        }

        var estadoNuevo = await _solicitudRepository.GetEstadoByCodigoAsync(solicitud.Estado.Codigo);
        if (estadoNuevo == null) return;

        string? url = null;
        string? triggerSource = null;

        if (estadoNuevo.Codigo == "SOL-APR")
        {
            url = "https://nacional-seguros-dev.isia.cloud/webhook/ares-generador-pdf";
            triggerSource = "BACKOFFICE_APPROVED";
        }

        if (url != null && triggerSource != null)
        {
            // Obtener el ID del usuario que realizó el cambio
            int performedByUserId = solicitud.SolicitanteId;
            var usuarioChanger = await _usuarioRepository.GetByCorreoAsync(ev.Changer);
            if (usuarioChanger != null)
            {
                performedByUserId = usuarioChanger.Id;
            }

            var payload = new
            {
                vacancyRequestId = solicitud.Id,
                triggerSource = triggerSource,
                performedByUserId = performedByUserId,
                correlationId = solicitud.CorrelationId?.ToString() ?? Guid.NewGuid().ToString()
            };

            // Disparar de forma asíncrona fire-and-forget
            _ = EnviarWebhookAsync(url, payload, solicitud.Id, null, solicitud.CorrelationId?.ToString());

            // Enviar webhook específico para perfil vacante (n8n)
            var n8nWebhookUrl = "https://nacional-seguros-dev.isia.cloud/webhook/perfil_vacante";
            var n8nPayload = new
            {
                solicitudId = solicitud.Id,
                codigoSolicitud = solicitud.Codigo,
                codigoPerfil = $"PRF-{solicitud.Codigo}",
                cargoRequerido = solicitud.Cargo,
                solicitanteNombre = solicitud.Solicitante?.Nombre ?? string.Empty,
                cargoDelSolicitante = solicitud.Solicitante?.Cargo ?? string.Empty,
                area = solicitud.Solicitante?.Area?.Nombre ?? string.Empty,
                regionalCiudad = solicitud.Regional?.Nombre ?? string.Empty,
                tipoSolicitud = solicitud.TipoSolicitud?.Nombre ?? string.Empty,
                motivo = solicitud.Motivo ?? string.Empty,
                modalidad = solicitud.ModalidadTrabajo?.Nombre ?? string.Empty,
                cantidadVacantes = solicitud.CantidadVacantes,
                prioridad = solicitud.Prioridad,
                seniority = solicitud.Seniority,
                objetivoPrincipal = solicitud.ObjetivoCargo,
                funciones = solicitud.Funciones ?? string.Empty,
                funcionesPrincipales = solicitud.Funciones ?? string.Empty,
                formacionAcademica = solicitud.FormacionAcademica,
                experienciaMinima = solicitud.ExperienciaMinima,
                experienciaIndispensable = solicitud.ExperienciaIndispensable,
                herramientasSistemas = solicitud.HerramientasSistemas ?? string.Empty,
                competenciasClave = solicitud.CompetenciasClave ?? string.Empty,
                disponibilidadRequerida = solicitud.DisponibilidadRequerida ?? string.Empty,
                criteriosExcluyentes = solicitud.CriteriosExcluyentes,
                criteriosDeseables = solicitud.CriteriosDeseables,
                conocimientosTecnicosRequeridos = solicitud.ConocimientosTecnicos,
                observaciones = solicitud.Observaciones ?? string.Empty,
                origen = "BACKOFFICE_APPROVED",
                correlationId = solicitud.CorrelationId?.ToString() ?? Guid.NewGuid().ToString()
            };
            _ = EnviarWebhookAsync(n8nWebhookUrl, n8nPayload, solicitud.Id, null, solicitud.CorrelationId?.ToString(), "Aprobación Solicitud n8n");
        }

        // Webhook de Notificación para Decisiones de RRHH (Aprobar, Rechazar, Observar)
        if (estadoNuevo.Codigo == "SOL-APR" || estadoNuevo.Codigo == "SOL-RECH" || estadoNuevo.Codigo == "SOL-OBS")
        {
            if (estadoNuevo.Codigo == "SOL-OBS" && _unitOfWork.SkipWhatsAppNotification)
            {
                _logger.LogInformation("Webhook: Omitiendo envío de WhatsApp de observación por decisión del usuario (SkipWhatsAppNotification = true) para solicitud {SolicitudId}.", solicitud.Id);
            }
            else
            {
                var usuarioChanger = await _usuarioRepository.GetByCorreoAsync(ev.Changer);
                if (usuarioChanger != null && System.Linq.Enumerable.Any(usuarioChanger.Roles, r => string.Equals(r.Nombre, "RRHH", StringComparison.OrdinalIgnoreCase)))
                {
                    var rrhhWebhookUrl = "https://nacional-seguros-dev.isia.cloud/webhook/Notificador_Aprobado_Rechazado_Observado_SIR";
                    var rrhhPayload = new { id = solicitud.Id };
                    _ = EnviarWebhookAsync(rrhhWebhookUrl, rrhhPayload, solicitud.Id, null, solicitud.CorrelationId?.ToString());
                }
            }
        }
    }

    private async Task EnviarWebhookAsync(
        string url, 
        object payload, 
        int solicitudId, 
        string? origen = null, 
        string? correlationId = null, 
        string tipoEvento = "Transición")
    {
        var fechaHora = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Registrar antes de enviar el webhook
        _logger.LogInformation(
            "Iniciando envío de webhook. SolicitudId: {SolicitudId}, Origen: {Origen}, CorrelacionId: {CorrelationId}, URL: {Url}, Fecha: {FechaHora}",
            solicitudId, origen ?? "N/A", correlationId ?? "N/A", url, fechaHora);

        try
        {
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await HttpClient.PostAsync(url, content);
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Auditoría Webhook: Fecha/Hora: {FechaHora}, URL: {Url}, Tipo Evento: {TipoEvento}, SolicitudId: {SolicitudId}, Origen: {Origen}, CorrelacionId: {CorrelationId}, Estado: Exitoso, Código HTTP: {StatusCode}, TiempoEjecucion: {ElapsedMs}ms, Reintentos: 0, Error: Ninguno",
                fechaHora, url, tipoEvento, solicitudId, origen ?? "N/A", correlationId ?? "N/A", response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Auditoría Webhook (FALLIDO): Fecha/Hora: {FechaHora}, URL: {Url}, Tipo Evento: {TipoEvento}, SolicitudId: {SolicitudId}, Origen: {Origen}, CorrelacionId: {CorrelationId}, Estado: Fallido, Código HTTP: N/A, TiempoEjecucion: {ElapsedMs}ms, Reintentos: 0, Error: {MensajeError}",
                fechaHora, url, tipoEvento, solicitudId, origen ?? "N/A", correlationId ?? "N/A", stopwatch.ElapsedMilliseconds, ex.Message);
        }
    }
}
