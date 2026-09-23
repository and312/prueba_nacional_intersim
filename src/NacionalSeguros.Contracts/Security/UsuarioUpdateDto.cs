using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Security;

public class UsuarioUpdateDto
{
    public string Correo { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public int AreaId { get; set; }
    public string? Cargo { get; set; }
    public string? Gerencia { get; set; }
    public string? Telefono { get; set; }
    public string? Extension { get; set; }
    public string? Observaciones { get; set; }
    public string? FotografiaUrl { get; set; }
    public List<int> RolIds { get; set; } = new();
    public string Estado { get; set; } = "Activo"; // Activo, Inactivo, Bloqueado
}
