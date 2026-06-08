using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Idkollen.Client;

internal sealed class Transport
{
    private const string UserAgent = "idkollen-client-csharp/0.1.0";

    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _authHeader;
    public JsonSerializerOptions JsonOptions { get; }

    public Transport(HttpClient http, string baseUrl, string clientId, string clientSecret, JsonSerializerOptions jsonOptions)
    {
        _http = http;
        _baseUrl = baseUrl.TrimEnd('/');
        var creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        _authHeader = $"Basic {creds}";
        JsonOptions = jsonOptions;
    }

    public async Task<T> PostAsync<T>(string path, object body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        using var req = BuildRequest(HttpMethod.Post, path);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return await SendAsync<T>(req, ct).ConfigureAwait(false);
    }

    public Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        var req = BuildRequest(HttpMethod.Get, path);
        return SendAsync<T>(req, ct);
    }

    public async Task<byte[]> GetRawAsync(string path, CancellationToken ct)
    {
        using var req = BuildRequest(HttpMethod.Get, path);
        using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode) throw await BuildErrorAsync(resp, body, ct).ConfigureAwait(false);
        return body;
    }

    public async Task DeleteAsync(string path, CancellationToken ct)
    {
        using var req = BuildRequest(HttpMethod.Delete, path);
        using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            throw await BuildErrorAsync(resp, body, ct).ConfigureAwait(false);
        }
    }

    public async Task<T> PostMultipartAsync<T>(string path, byte[] data, string filename, string mimeType, CancellationToken ct)
    {
        using var req = BuildRequest(HttpMethod.Post, path);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(data);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        content.Add(fileContent, "file", filename);
        req.Content = content;
        return await SendAsync<T>(req, ct).ConfigureAwait(false);
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string path)
    {
        var req = new HttpRequestMessage(method, _baseUrl + path);
        req.Headers.TryAddWithoutValidation("Authorization", _authHeader);
        req.Headers.UserAgent.ParseAdd(UserAgent);
        return req;
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage req, CancellationToken ct)
    {
        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            throw new IdkollenException(0, e.Message);
        }

        try
        {
            var body = await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) throw await BuildErrorAsync(resp, body, ct).ConfigureAwait(false);
            if (body.Length == 0) return default!;
            return JsonSerializer.Deserialize<T>(body, JsonOptions)!;
        }
        finally
        {
            resp.Dispose();
        }
    }

    private static Task<IdkollenException> BuildErrorAsync(HttpResponseMessage resp, byte[] body, CancellationToken _)
    {
        string message;
        try
        {
            using var doc = JsonDocument.Parse(body);
            message = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() ?? "" : Encoding.UTF8.GetString(body);
        }
        catch
        {
            message = Encoding.UTF8.GetString(body);
        }
        return Task.FromResult(new IdkollenException((int)resp.StatusCode, message));
    }
}
