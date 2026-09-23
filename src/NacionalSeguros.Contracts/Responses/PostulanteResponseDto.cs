using System;

namespace NacionalSeguros.Contracts.Responses;

public record PostulanteResponseDto(
    int PostulanteId,
    string Correo,
    string EstadoNombre,
    DateTime FechaRegistro);
