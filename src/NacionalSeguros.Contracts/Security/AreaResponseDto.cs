using System;

namespace NacionalSeguros.Contracts.Security;

public class AreaResponseDto
{
    public int AreaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int GerenciaId { get; set; }
    public string Gerencia { get; set; } = string.Empty; // Nombre de la Gerencia relacionada
    public string? Responsable { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int CantidadUsuarios { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
