namespace NacionalSeguros.Contracts.Responses;

public record TokenResponse(string AccessToken, string RefreshToken, DateTime Expiration);
