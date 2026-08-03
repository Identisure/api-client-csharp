namespace Idkollen.Client;

public enum IdkollenEnvironment
{
    Production,
    Staging,
}

internal static class IdkollenEnvironmentExtensions
{
    public static string BaseUrl(this IdkollenEnvironment env) => env switch
    {
        IdkollenEnvironment.Production => "https://api.idkollen.se",
        IdkollenEnvironment.Staging => "https://stgapi.idkollen.se",
        _ => throw new ArgumentOutOfRangeException(nameof(env)),
    };
}
