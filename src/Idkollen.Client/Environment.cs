namespace Idkollen.Client;

public enum Environment
{
    Production,
    Staging,
}

internal static class EnvironmentExtensions
{
    public static string BaseUrl(this Environment env) => env switch
    {
        Environment.Production => "https://api.idkollen.se",
        Environment.Staging => "https://stgapi.idkollen.se",
        _ => throw new ArgumentOutOfRangeException(nameof(env)),
    };
}
