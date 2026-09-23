using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.GetWhatsAppCostCheck;

public class GetWhatsAppCostCheckQueryHandler : IRequestHandler<GetWhatsAppCostCheckQuery, Result<WhatsAppCostCheckResponseDto>>
{
    private readonly ISolicitudRepository _solicitudRepository;

    public GetWhatsAppCostCheckQueryHandler(ISolicitudRepository solicitudRepository)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
    }

    public async Task<Result<WhatsAppCostCheckResponseDto>> Handle(GetWhatsAppCostCheckQuery request, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(request.Id);
        if (solicitud == null)
        {
            return Result.Failure<WhatsAppCostCheckResponseDto>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Id} no existe."));
        }

        var fechaUltimaActualizacion = await _solicitudRepository.GetFechaUltimaInteraccionSolicitanteAsync(request.Id);
        var refDate = fechaUltimaActualizacion ?? solicitud.CreatedDate;

        var horasTranscurridas = (DateTime.UtcNow - refDate).TotalHours;
        if (horasTranscurridas < 0) horasTranscurridas = 0; // Evitar desfases de reloj

        bool requiereConfirmacion = horasTranscurridas > 24.0;
        string mensajeTiempo = FormatearMensajeTiempo(horasTranscurridas);
        string estadoConversacion = requiereConfirmacion ? "Expirada" : "Activa";

        var responseDto = new WhatsAppCostCheckResponseDto(
            FechaReferencia: refDate,
            HorasTranscurridas: Math.Round(horasTranscurridas, 2),
            RequiereConfirmacion: requiereConfirmacion,
            FechaCreacion: solicitud.CreatedDate,
            FechaUltimaActualizacionSolicitante: fechaUltimaActualizacion,
            HorasDesdeUltimaActualizacion: Math.Round(horasTranscurridas, 2),
            RequiereConfirmacionWhatsapp: requiereConfirmacion,
            MensajeTiempo: mensajeTiempo,
            EstadoConversacionWhatsapp: estadoConversacion
        );

        return Result.Success(responseDto);
    }

    private static string FormatearMensajeTiempo(double totalHours)
    {
        var span = TimeSpan.FromHours(totalHours);
        int days = span.Days;
        int hours = span.Hours;

        if (days == 0)
        {
            return $"Hace {hours} {(hours == 1 ? "hora" : "horas")}";
        }
        else
        {
            string dayText = days == 1 ? "día" : "días";
            if (hours == 0)
            {
                return $"Hace {days} {dayText}";
            }
            string hourText = hours == 1 ? "hora" : "horas";
            return $"Hace {days} {dayText} {hours} {hourText}";
        }
    }
}
