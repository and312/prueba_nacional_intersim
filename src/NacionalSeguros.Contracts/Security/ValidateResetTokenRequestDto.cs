namespace NacionalSeguros.Contracts.Security;

public class ValidateResetTokenRequestDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}
