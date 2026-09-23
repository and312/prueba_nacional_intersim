using System;

namespace NacionalSeguros.Contracts.Responses;

public record UltimaObservacionRrhhDto(
    string Texto,
    DateTime Fecha,
    string Usuario
);
