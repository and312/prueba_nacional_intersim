using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Security;

public class UsuarioCreateDto
{
    public string Correo { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoAutenticacion { get; set; } = "Local"; // "Local" o "ActiveDirectory"
    public string? Clave { get; set; }
    public int AreaId { get; set; }
    public string? Cargo { get; set; }
    public string? Gerencia { get; set; }
    public string? Telefono { get; set; }
    public string? Extension { get; set; }
    public string? Observaciones { get; set; }
    public string? FotografiaUrl { get; set; }
    public List<int> RolIds { get; set; } = new();
}
