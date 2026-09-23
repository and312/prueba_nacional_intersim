namespace NacionalSeguros.Contracts.Security;

public class ChangePasswordRequestDto
{
    public string ClaveActual { get; set; } = string.Empty;
    public string NuevaClave { get; set; } = string.Empty;
}
