using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.RegenerarPdf;

public class RegenerarPdfCommandHandler : IRequestHandler<RegenerarPdfCommand, Result>
{
    private static readonly HttpClient HttpClient = new();
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<RegenerarPdfCommandHandler> _logger;

    public RegenerarPdfCommandHandler(
        IPerfilCargoRepository perfilCargoRepository,
        ISolicitudRepository solicitudRepository,
        IUsuarioRepository usuarioRepository,
        ILogger<RegenerarPdfCommandHandler> logger)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(RegenerarPdfCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilCargoRepository.GetByIdAsync(request.PerfilCargoId);
        if (perfil == null)
        {
            return Result.Failure(new Error("PerfilCargo.NotFound", $"El perfil de cargo con ID {request.PerfilCargoId} no existe."));
        }

        var solicitud = await _solicitudRepository.GetByIdAsync(perfil.SolicitudId);
        if (solicitud == null)
        {
            return Result.Failure(new Error("Solicitud.NotFound", $"La solicitud con ID {perfil.SolicitudId} no existe."));
        }

        int performedByUserId = solicitud.SolicitanteId;
        var usuario = await _usuarioRepository.GetByCorreoAsync(request.UserEmail);
        if (usuario != null)
        {
            performedByUserId = usuario.Id;
        }

        string correlationId = solicitud.CorrelationId?.ToString() ?? Guid.NewGuid().ToString();
        string url = "https://nacional-seguros-dev.isia.cloud/webhook/ares-generador-pdf";

        var payload = new
        {
            vacancyRequestId = solicitud.Id,
            triggerSource = "BACKOFFICE_REGENERATED",
            performedByUserId = performedByUserId,
            correlationId = correlationId
        };

        // Disparar de forma asíncrona fire-and-forget
        _ = Task.Run(async () =>
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogInformation("Enviando webhook de regeneración de PDF para SolicitudId: {SolicitudId}...", solicitud.Id);
                var response = await HttpClient.PostAsync(url, content, CancellationToken.None);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook de regeneración de PDF enviado exitosamente. Status: {StatusCode}", response.StatusCode);
                }
                else
                {
                    _logger.LogError("Webhook de regeneración de PDF falló con código: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar webhook de regeneración de PDF");
            }
        }, CancellationToken.None);

        return Result.Success();
    }
}
