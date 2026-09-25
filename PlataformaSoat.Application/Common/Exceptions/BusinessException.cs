namespace PlataformaSoat.Application.Common.Exceptions;

public class BusinessException : Exception
{
    public int ErrorCode { get; }

    public BusinessException(string message, int errorCode = 400) : base(message)
    {
        ErrorCode = errorCode;
    }
}
