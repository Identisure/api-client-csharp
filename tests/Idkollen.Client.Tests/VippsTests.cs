using System.Net;
using System.Text.Json;
using Idkollen.Client.Models;
using Xunit;

namespace Idkollen.Client.Tests;

public class VippsTests
{
    private static (Idkollen.Client.IdkollenClient Client, MockHttpMessageHandler Handler) Build()
    {
        var handler = new MockHttpMessageHandler();
        var http = new HttpClient(handler);
        var client = new Idkollen.Client.IdkollenClientBuilder("cid", "sec")
            .BaseUrl("https://x.test").HttpClient(http).Build();
        return (client, handler);
    }

    [Fact]
    public async Task AuthPending()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new { status = "PENDING", id = "v1", url = "https://login" });
        var result = await client.Vipps.AuthAsync(new VippsAuthRequest());
        Assert.IsType<VippsPending>(result);
    }

    [Fact]
    public async Task BackchannelAuthSendsPhone()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new { status = "PENDING", id = "v1" });
        await client.Vipps.BackchannelAuthAsync(new VippsBackchannelAuthRequest("+4712345678"));
        using var doc = JsonDocument.Parse(handler.LastRequestBody);
        Assert.Equal("+4712345678", doc.RootElement.GetProperty("phone").GetString());
    }
}
