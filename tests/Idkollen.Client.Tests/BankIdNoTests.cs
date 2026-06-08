using System.Net;
using Idkollen.Client.Models;
using Xunit;

namespace Idkollen.Client.Tests;

public class BankIdNoTests
{
    private static (Idkollen.Client.IdkollenClient Client, MockHttpMessageHandler Handler) Build()
    {
        var handler = new MockHttpMessageHandler();
        var http = new HttpClient(handler);
        var client = new Idkollen.Client.IdkollenClientBuilder("cid", "sec")
            .BaseUrl("https://x.test")
            .HttpClient(http)
            .Build();
        return (client, handler);
    }

    [Fact]
    public async Task AuthPending()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new { status = "PENDING", id = "no1", url = "https://login" });
        var result = await client.BankIdNo.AuthAsync(new BankIdNoAuthRequest { RequestSsn = true });
        var pending = Assert.IsType<BankIdNoPending>(result);
        Assert.Equal("https://login", pending.Url);
    }

    [Fact]
    public async Task AuthCompletedWithSignedDocuments()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new
        {
            status = "COMPLETED",
            id = "no1",
            ssn = "01010100000",
            name = "Ola Nordmann",
            givenName = "Ola",
            surname = "Nordmann",
            signedDocuments = new[]
            {
                new { id = "d1", hash = "h1" },
                new { id = "d2", hash = "h2" },
            },
        });

        var result = await client.BankIdNo.AuthStatusAsync("no1");
        var done = Assert.IsType<BankIdNoCompleted>(result);
        Assert.Equal(2, done.SignedDocuments!.Count);
        Assert.Equal("d1", done.SignedDocuments[0].Id);
    }
}
