namespace NacionalSeguros.Contracts.Security;

public class ApiErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
