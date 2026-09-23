namespace NacionalSeguros.Contracts.Security;

public class GerenciaResponseDto
{
    public int GerenciaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
