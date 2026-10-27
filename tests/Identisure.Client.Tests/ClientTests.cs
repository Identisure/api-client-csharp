using System.Net;
using Identisure.Client;
using Xunit;

namespace Identisure.Client.Tests;

public class ClientTests
{
    [Fact]
    public void BuilderProducesClient()
    {
        var client = new IdentisureClientBuilder("id", "secret")
            .Environment(IdentisureEnvironment.Staging)
            .Build();
        Assert.NotNull(client);
    }

    [Fact]
    public async Task TransportAttachesBasicAuthAndUserAgent()
    {
        var handler = new MockHttpMessageHandler();
        handler.Enqueue(HttpStatusCode.OK, new { ok = true });
        var http = new HttpClient(handler);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var transport = new Transport(http, "https://example.test", "cid", "sec", jsonOptions, TimeSpan.FromSeconds(30));

        await transport.GetAsync<Dictionary<string, object>>("/v3/ping", default);

        var req = handler.LastRequest;
        Assert.Equal("Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("cid:sec")), req.Headers.GetValues("Authorization").First());
        Assert.Contains("identisure-client-csharp/", req.Headers.UserAgent.ToString());
        Assert.Equal("https://example.test/v3/ping", req.RequestUri!.ToString());
    }

    [Fact]
    public async Task Non2xxThrowsIdentisureException()
    {
        var handler = new MockHttpMessageHandler();
        handler.Enqueue(HttpStatusCode.BadRequest, new { message = "bad request" });
        var http = new HttpClient(handler);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions();
        var transport = new Transport(http, "https://x.test", "a", "b", jsonOptions, TimeSpan.FromSeconds(30));

        var ex = await Assert.ThrowsAsync<IdentisureException>(() => transport.PostAsync<object>("/v3/things", new { a = 1 }, default));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("bad request", ex.Message);
    }

    [Fact]
    public async Task TimeoutThrowsIdentisureException()
    {
        var client = new IdentisureClientBuilder("a", "b")
            .BaseUrl("https://x.test")
            .HttpClient(new HttpClient(new HangingHandler()))
            .Timeout(TimeSpan.FromMilliseconds(50))
            .Build();

        var ex = await Assert.ThrowsAsync<IdentisureException>(() => client.Ftn.AuthStatusAsync("id"));
        Assert.Equal("Request timed out", ex.Message);
    }

    [Fact]
    public async Task CallerCancellationPropagates()
    {
        var client = new IdentisureClientBuilder("a", "b")
            .BaseUrl("https://x.test")
            .HttpClient(new HttpClient(new HangingHandler()))
            .Build();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.Ftn.AuthStatusAsync("id", cts.Token));
    }

    private sealed class HangingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(System.Threading.Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Unreachable");
        }
    }
}
