namespace NacionalSeguros.Contracts.Security;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int ExpiraEnSegundos { get; set; }
    public UsuarioResponseDto? Usuario { get; set; }
    public bool MfaRequerido { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
