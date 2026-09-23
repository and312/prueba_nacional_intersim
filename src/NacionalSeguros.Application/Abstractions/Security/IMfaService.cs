namespace NacionalSeguros.Application.Abstractions.Security;

public interface IMfaService
{
    string GenerateSecretKey();
    bool ValidateCode(string secret, string code);
    string GetQrCodeUri(string correo, string secret);
}
