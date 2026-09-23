using System;

namespace NacionalSeguros.Contracts.Responses;

public class PerfilHabilitadoDto
{
    public int PerfilCargoId { get; set; }
    public int MatchingEjecucionId { get; set; }
    public string CodigoMatching { get; set; } = string.Empty;
    public string CodigoPerfil { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public string EstadoCodigo { get; set; } = string.Empty;
    public string EstadoNombre { get; set; } = string.Empty;
    public DateTime FechaEstrategia { get; set; }
}
