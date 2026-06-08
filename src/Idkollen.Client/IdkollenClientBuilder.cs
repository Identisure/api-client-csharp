using System.Text.Json;

namespace Idkollen.Client;

public sealed class IdkollenClientBuilder
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private Idkollen.Client.Environment _environment = Idkollen.Client.Environment.Production;
    private string? _baseUrl;
    private HttpClient? _httpClient;

    public IdkollenClientBuilder(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    public IdkollenClientBuilder Environment(Idkollen.Client.Environment env)
    {
        _environment = env;
        return this;
    }

    public IdkollenClientBuilder BaseUrl(string url)
    {
        _baseUrl = url;
        return this;
    }

    public IdkollenClientBuilder HttpClient(HttpClient client)
    {
        _httpClient = client;
        return this;
    }

    public IdkollenClient Build()
    {
        var http = _httpClient ?? new HttpClient();
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
        var transport = new Transport(http, baseUrl, _clientId, _clientSecret, jsonOptions);
        return new IdkollenClient(transport);
    }
}
