namespace NacionalSeguros.Contracts.Security;

public class RolResponseDto
{
    public int RolId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public int CantidadUsuarios { get; set; }
    public System.Collections.Generic.List<string> Permisos { get; set; } = new();
}
