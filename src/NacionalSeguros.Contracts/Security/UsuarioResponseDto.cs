using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Security;

public class UsuarioResponseDto
{
    public int UsuarioId { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoAutenticacion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string Estado { get; set; } = string.Empty; // Activo, Inactivo, Bloqueado
    public int AreaId { get; set; }
    public string AreaNombre { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Gerencia { get; set; }
    public string? Telefono { get; set; }
    public string? Extension { get; set; }
    public string? Observaciones { get; set; }
    public string? FotografiaUrl { get; set; }
    public System.DateTime FechaCreacion { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permisos { get; set; } = new();
}
