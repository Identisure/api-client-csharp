namespace Idkollen.Client;

public class IdkollenException : Exception
{
    public int StatusCode { get; }

    public IdkollenException(int statusCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
