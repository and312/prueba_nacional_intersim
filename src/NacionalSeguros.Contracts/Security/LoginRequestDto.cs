namespace NacionalSeguros.Contracts.Security;

public class LoginRequestDto
{
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string TipoAutenticacion { get; set; } = "Local"; // "Local" o "ActiveDirectory"
}
