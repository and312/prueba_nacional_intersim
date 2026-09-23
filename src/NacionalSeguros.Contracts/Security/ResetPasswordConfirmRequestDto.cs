namespace NacionalSeguros.Contracts.Security;

public class ResetPasswordConfirmRequestDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; } = string.Empty;
}
