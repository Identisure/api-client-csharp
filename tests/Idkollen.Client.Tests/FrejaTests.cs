using System.Net;
using Idkollen.Client.Models;
using Xunit;

namespace Idkollen.Client.Tests;

public class FrejaTests
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
        handler.Enqueue(HttpStatusCode.OK, new { status = "PENDING", id = "f1", autoStartToken = "tok", qrData = "qr" });
        var result = await client.Freja.AuthAsync(new FrejaAuthRequest());
        var p = Assert.IsType<FrejaPending>(result);
        Assert.Equal("qr", p.QrData);
    }

    [Fact]
    public async Task AuthCompleted()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new
        {
            status = "COMPLETED", id = "f1", ssn = "199001011234",
            country = "SE", name = "Test", givenName = "T", surname = "est",
        });
        var result = await client.Freja.AuthStatusAsync("f1");
        var c = Assert.IsType<FrejaCompleted>(result);
        Assert.Equal("SE", c.Country);
    }
}
