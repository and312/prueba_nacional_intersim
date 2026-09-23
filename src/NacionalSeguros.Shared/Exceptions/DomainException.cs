namespace NacionalSeguros.Shared.Exceptions;

public class DomainException : BaseException
{
    public DomainException(string code, string message)
        : base("Violación de Regla de Negocio", message)
    {
        Code = code;
    }

    public string Code { get; }
}
