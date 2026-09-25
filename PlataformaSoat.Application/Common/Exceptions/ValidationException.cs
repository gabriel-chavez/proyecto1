namespace PlataformaSoat.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Failures { get; } = new Dictionary<string, string[]>();

    public ValidationException() 
        : base("Uno o más errores de validación han ocurrido.")
    {
    }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(IDictionary<string, string[]> failures) : this()
    {
        Failures = failures;
    }
}
