using System.Net;
using Idkollen.Client.Models;
using Xunit;

namespace Idkollen.Client.Tests;

public class MitIdTests
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
        handler.Enqueue(HttpStatusCode.OK, new { status = "PENDING", id = "m1", url = "https://login" });
        var result = await client.MitId.AuthAsync(new MitIdAuthRequest());
        Assert.IsType<MitIdPending>(result);
    }

    [Fact]
    public async Task SignCompletedWithSignResult()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new
        {
            status = "COMPLETED", id = "m1", ssn = "0101010000",
            name = "Hans", givenName = "Hans", surname = "H",
            signResult = new { checksum = "abc123" },
        });
        var result = await client.MitId.SignStatusAsync("m1");
        var done = Assert.IsType<MitIdCompleted>(result);
        Assert.NotNull(done.SignResult);
        Assert.Equal("abc123", done.SignResult!.Checksum);
    }
}
