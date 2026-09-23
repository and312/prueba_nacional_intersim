using System;

namespace NacionalSeguros.Contracts.Responses;

public record WhatsAppCostCheckResponseDto(
    DateTime FechaReferencia,
    double HorasTranscurridas,
    bool RequiereConfirmacion,
    DateTime FechaCreacion,
    DateTime? FechaUltimaActualizacionSolicitante,
    double HorasDesdeUltimaActualizacion,
    bool RequiereConfirmacionWhatsapp,
    string MensajeTiempo,
    string EstadoConversacionWhatsapp);
