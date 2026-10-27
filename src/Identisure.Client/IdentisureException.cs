namespace Identisure.Client;

public class IdentisureException : Exception
{
    public int StatusCode { get; }

    public IdentisureException(int statusCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
