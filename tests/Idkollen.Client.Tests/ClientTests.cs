using System.Net;
using Idkollen.Client;
using Xunit;

namespace Idkollen.Client.Tests;

public class ClientTests
{
    [Fact]
    public void BuilderProducesClient()
    {
        var client = new IdkollenClientBuilder("id", "secret")
            .Environment(Idkollen.Client.Environment.Staging)
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
        var transport = new Transport(http, "https://example.test", "cid", "sec", jsonOptions);

        await transport.GetAsync<Dictionary<string, object>>("/v3/ping", default);

        var req = handler.LastRequest;
        Assert.Equal("Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("cid:sec")), req.Headers.GetValues("Authorization").First());
        Assert.Contains("idkollen-client-csharp/", req.Headers.UserAgent.ToString());
        Assert.Equal("https://example.test/v3/ping", req.RequestUri!.ToString());
    }

    [Fact]
    public async Task Non2xxThrowsIdkollenException()
    {
        var handler = new MockHttpMessageHandler();
        handler.Enqueue(HttpStatusCode.BadRequest, new { message = "bad request" });
        var http = new HttpClient(handler);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions();
        var transport = new Transport(http, "https://x.test", "a", "b", jsonOptions);

        var ex = await Assert.ThrowsAsync<IdkollenException>(() => transport.PostAsync<object>("/v3/things", new { a = 1 }, default));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("bad request", ex.Message);
    }
}
