using System;

namespace NacionalSeguros.Contracts.Responses;

public record VacanteResponseDto(
    int VacanteId,
    int SolicitudId,
    int PerfilId,
    string EstadoNombre,
    DateTime? FechaLimiteCobertura);
