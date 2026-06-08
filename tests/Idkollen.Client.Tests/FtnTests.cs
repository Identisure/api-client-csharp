using System.Net;
using Idkollen.Client.Models;
using Xunit;

namespace Idkollen.Client.Tests;

public class FtnTests
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
        handler.Enqueue(HttpStatusCode.OK, new { status = "PENDING", id = "f1", url = "https://login" });
        var result = await client.Ftn.AuthAsync(new FtnAuthRequest());
        var p = Assert.IsType<FtnPending>(result);
        Assert.Equal("https://login", p.Url);
    }

    [Fact]
    public async Task AgeVerificationCompleted()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new { status = "COMPLETED", id = "av1", ageVerified = true });
        var result = await client.Ftn.AgeVerificationAsync(new AgeVerificationRequest { MinAge = 18 });
        var done = Assert.IsType<AgeVerificationCompleted>(result);
        Assert.True(done.AgeVerified);
    }
}
