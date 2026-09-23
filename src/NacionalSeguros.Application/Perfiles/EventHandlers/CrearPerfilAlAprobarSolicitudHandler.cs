using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NacionalSeguros.Application.Abstractions.Events;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Events;
using NacionalSeguros.Domain.Repositories;

namespace NacionalSeguros.Application.Perfiles.EventHandlers;

public class CrearPerfilAlAprobarSolicitudHandler : INotificationHandler<DomainEventNotification<SolicitudEstadoTransitadoEvent>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CrearPerfilAlAprobarSolicitudHandler> _logger;

    public CrearPerfilAlAprobarSolicitudHandler(
        IPerfilCargoRepository perfilCargoRepository,
        ISolicitudRepository solicitudRepository,
        IUnitOfWork unitOfWork,
        ILogger<CrearPerfilAlAprobarSolicitudHandler> logger)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<SolicitudEstadoTransitadoEvent> notification, CancellationToken cancellationToken)
    {
        var ev = notification.DomainEvent;
        _logger.LogInformation("[CrearPerfilHandler] Received SolicitudEstadoTransitadoEvent for SolicitudId: {SolicitudId}, NuevoEstadoId: {NuevoEstadoId}", ev.SolicitudId, ev.EstadoNuevoId);
        
        var solicitud = await _solicitudRepository.GetByIdAsync(ev.SolicitudId);
        if (solicitud == null)
        {
            _logger.LogWarning("[CrearPerfilHandler] Solicitud not found for SolicitudId: {SolicitudId}", ev.SolicitudId);
            return;
        }

        _logger.LogInformation("[CrearPerfilHandler] Solicitud Estado Codigo: {EstadoCodigo}", solicitud.Estado?.Codigo);

        if (solicitud.Estado?.Codigo == "SOL-APR")
        {
            // Verificar si ya existe un perfil para esta solicitud para evitar duplicados
            var existing = await _perfilCargoRepository.GetLatestBySolicitudIdAsync(solicitud.Id);
            _logger.LogInformation("[CrearPerfilHandler] Existing PerfilCargo: {Existing}", existing != null ? "Found (ID: " + existing.Id + ")" : "None");
            if (existing != null) return;

            var estadoPendiente = await _perfilCargoRepository.GetEstadoByCodigoAsync("PERF-PEN-GEN");
            if (estadoPendiente == null)
            {
                _logger.LogError("[CrearPerfilHandler] Estado 'PERF-PEN-GEN' not found in database!");
                return;
            }

            _logger.LogInformation("[CrearPerfilHandler] Found target EstadoId: {EstadoId} for 'PERF-PEN-GEN'", estadoPendiente.Id);

            // Establecer el comentario de transición para que el interceptor lo grabe en StateHistory
            _unitOfWork.TransitionComment = "Creación de perfil pendiente tras aprobación de solicitud";

            var perfil = new PerfilCargo(
                solicitudId: solicitud.Id,
                cargo: solicitud.Cargo,
                descripcion: "{}",
                version: 1,
                estadoId: estadoPendiente.Id,
                createdBy: "System"
            );

            _logger.LogInformation("[CrearPerfilHandler] Creating PerfilCargo entity for SolicitudId: {SolicitudId}", solicitud.Id);
            await _perfilCargoRepository.AddAsync(perfil);
            perfil.SetCodigo($"PRF-{solicitud.Codigo}");
            
            _logger.LogInformation("[CrearPerfilHandler] Saving changes to unit of work...");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[CrearPerfilHandler] Successfully saved PerfilCargo. Created PerfilId: {PerfilId}", perfil.Id);
        }
    }
}

