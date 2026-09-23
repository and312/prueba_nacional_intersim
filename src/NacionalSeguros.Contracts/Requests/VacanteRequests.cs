using System;

namespace NacionalSeguros.Contracts.Requests;

public record VacanteCreateDto(
    int SolicitudId,
    int PerfilId,
    DateTime FechaLimiteCobertura);
