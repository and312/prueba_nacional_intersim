namespace NacionalSeguros.Contracts.Security;

public class MfaVerifyRequestDto
{
    public string Correo { get; set; } = string.Empty;
    public string CodigoOtp { get; set; } = string.Empty;
}
