namespace Idkollen.Client;

public class IdkollenException : Exception
{
    public int StatusCode { get; }

    public IdkollenException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
