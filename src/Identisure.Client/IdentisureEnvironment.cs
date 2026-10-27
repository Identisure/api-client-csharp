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
        IdentisureEnvironment.Production => "https://api.identisure.com",
        IdentisureEnvironment.Staging => "https://stgapi.identisure.com",
        _ => throw new ArgumentOutOfRangeException(nameof(env)),
    };
}
