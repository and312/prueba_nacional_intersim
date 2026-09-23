namespace NacionalSeguros.Shared.Primitives;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "El valor especificado es nulo.");
}
