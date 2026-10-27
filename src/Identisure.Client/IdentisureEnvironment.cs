namespace Identisure.Client;

public enum IdentisureEnvironment
{
    Production,
    Staging,
}

internal static class IdentisureEnvironmentExtensions
{
    public static string BaseUrl(this IdentisureEnvironment env) => env switch
    {
        IdentisureEnvironment.Production => "https://api.idkollen.se",
        IdentisureEnvironment.Staging => "https://stgapi.idkollen.se",
        _ => throw new ArgumentOutOfRangeException(nameof(env)),
    };
}
