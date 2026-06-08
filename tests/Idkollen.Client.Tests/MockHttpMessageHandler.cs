using System.Net;
using System.Text;

namespace Idkollen.Client.Tests;

internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode Status, string Body)> _queue = new();
    private readonly List<HttpRequestMessage> _requests = new();
    private readonly List<string> _requestBodies = new();

    public void Enqueue(HttpStatusCode status, string body)
    {
        _queue.Enqueue((status, body));
    }

    public void Enqueue(HttpStatusCode status, object body)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        _queue.Enqueue((status, json));
    }

    public HttpRequestMessage LastRequest => _requests[^1];
    public string LastRequestBody => _requestBodies[^1];
    public IReadOnlyList<HttpRequestMessage> Requests => _requests;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        _requests.Add(request);
        _requestBodies.Add(request.Content == null ? "" : await request.Content.ReadAsStringAsync(ct).ConfigureAwait(false));
        if (_queue.Count == 0) throw new InvalidOperationException("No responses queued");
        var (status, body) = _queue.Dequeue();
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }
}
