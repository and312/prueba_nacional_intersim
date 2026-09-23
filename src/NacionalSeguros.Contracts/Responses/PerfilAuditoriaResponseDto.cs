using System;

namespace NacionalSeguros.Contracts.Responses;

public class PerfilAuditoriaResponseDto
{
    public int PerfilAuditoriaId { get; set; }
    public int PerfilCargoId { get; set; }
    public int? PerfilSeccionId { get; set; }
    public int Version { get; set; }
    public string? SeccionModificada { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string? MotivoCambio { get; set; }
    public string? EstadoPerfil { get; set; }
}
