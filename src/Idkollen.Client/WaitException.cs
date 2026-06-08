namespace Idkollen.Client;

public class WaitException : Exception
{
    public bool Timeout { get; }

    public WaitException(bool timeout, Exception? innerException = null)
        : base(timeout ? "Poll timed out" : "Poll error", innerException)
    {
        Timeout = timeout;
    }
}
