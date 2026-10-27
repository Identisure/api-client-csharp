using System.Text.Json;

namespace Identisure.Client;

public sealed class IdentisureClientBuilder
{
    // Shared so building many clients reuses one connection pool instead of exhausting sockets.
    private static readonly HttpClient SharedHttpClient = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    })
    {
        Timeout = System.Threading.Timeout.InfiniteTimeSpan,
    };

    private readonly string _clientId;
    private readonly string _clientSecret;
    private IdentisureEnvironment _environment = IdentisureEnvironment.Production;
    private string? _baseUrl;
    private HttpClient? _httpClient;
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);

    public IdentisureClientBuilder(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    public IdentisureClientBuilder Environment(IdentisureEnvironment env)
    {
        _environment = env;
        return this;
    }

    public IdentisureClientBuilder BaseUrl(string url)
    {
        _baseUrl = url;
        return this;
    }

    public IdentisureClientBuilder HttpClient(HttpClient client)
    {
        _httpClient = client;
        return this;
    }

    /// <summary>Timeout for each HTTP request. Defaults to 30 seconds.</summary>
    public IdentisureClientBuilder Timeout(TimeSpan timeout)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
        _timeout = timeout;
        return this;
    }

    public IdentisureClient Build()
    {
        var http = _httpClient ?? SharedHttpClient;
        var baseUrl = _baseUrl ?? _environment.BaseUrl();
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        jsonOptions.Converters.Add(new Models.AgeVerificationStatusConverter());
        jsonOptions.Converters.Add(new Models.BankIdSeStatusConverter());
        jsonOptions.Converters.Add(new Models.BankIdSePhoneStatusConverter());
        jsonOptions.Converters.Add(new Models.BankIdNoStatusConverter());
        jsonOptions.Converters.Add(new Models.FrejaStatusConverter());
        jsonOptions.Converters.Add(new Models.MitIdStatusConverter());
        jsonOptions.Converters.Add(new Models.FtnStatusConverter());
        jsonOptions.Converters.Add(new Models.VippsStatusConverter());
        var transport = new Transport(http, baseUrl, _clientId, _clientSecret, jsonOptions, _timeout);
        return new IdentisureClient(transport);
    }
}
