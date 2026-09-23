namespace NacionalSeguros.Contracts.Security;

public class TokenRefreshRequestDto
{
    public string TokenExpirado { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
