using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Identisure.Client;

internal sealed class Transport
{
    private const string UserAgent = "identisure-client-csharp/0.1.0";

    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _authHeader;
    private readonly TimeSpan _timeout;
    public JsonSerializerOptions JsonOptions { get; }

    public Transport(HttpClient http, string baseUrl, string clientId, string clientSecret, JsonSerializerOptions jsonOptions, TimeSpan timeout)
    {
        _http = http;
        _baseUrl = baseUrl.TrimEnd('/');
        var creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        _authHeader = $"Basic {creds}";
        JsonOptions = jsonOptions;
        _timeout = timeout;
    }

    public async Task<T> PostAsync<T>(string path, object body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        using var req = BuildRequest(HttpMethod.Post, path);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return await SendAsync<T>(req, ct).ConfigureAwait(false);
    }

    public async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var req = BuildRequest(HttpMethod.Get, path);
        return await SendAsync<T>(req, ct).ConfigureAwait(false);
    }

    public async Task<byte[]> GetRawAsync(string path, CancellationToken ct)
    {
        using var req = BuildRequest(HttpMethod.Get, path);
        return await SendRawAsync(req, ct).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string path, CancellationToken ct)
    {
        using var req = BuildRequest(HttpMethod.Delete, path);
        await SendRawAsync(req, ct).ConfigureAwait(false);
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
        var body = await SendRawAsync(req, ct).ConfigureAwait(false);
        if (body.Length == 0) throw new IdentisureException(0, "Empty response body");
        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonOptions)
                ?? throw new IdentisureException(0, "Null response body");
        }
        catch (JsonException e)
        {
            throw new IdentisureException(0, e.Message, e);
        }
    }

    private async Task<byte[]> SendRawAsync(HttpRequestMessage req, CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(_timeout);
        try
        {
            using var resp = await _http.SendAsync(req, timeoutCts.Token).ConfigureAwait(false);
            var body = await resp.Content.ReadAsByteArrayAsync(timeoutCts.Token).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) throw await BuildErrorAsync(resp, body, ct).ConfigureAwait(false);
            return body;
        }
        catch (HttpRequestException e)
        {
            throw new IdentisureException(0, e.Message, e);
        }
        // The request timeout (ours or HttpClient.Timeout) surfaces as a cancellation the caller did not request.
        catch (OperationCanceledException e) when (!ct.IsCancellationRequested)
        {
            throw new IdentisureException(0, "Request timed out", e);
        }
    }

    private static Task<IdentisureException> BuildErrorAsync(HttpResponseMessage resp, byte[] body, CancellationToken _)
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
        return Task.FromResult(new IdentisureException((int)resp.StatusCode, message));
    }
}
