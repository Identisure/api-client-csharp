using System.Net;
using System.Text.Json;
using Idkollen.Client.Models;
using Xunit;

namespace Idkollen.Client.Tests;

public class BankIdSeTests
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
        handler.Enqueue(HttpStatusCode.OK, new { status = "PENDING", id = "abc", autoStartToken = "tok" });

        var result = await client.BankIdSe.AuthAsync(new BankIdSeAuthRequest { Ssn = "199001011234" });

        var pending = Assert.IsType<BankIdSePending>(result);
        Assert.Equal("abc", pending.Id);
        Assert.Equal("tok", pending.AutoStartToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody);
        Assert.Equal("199001011234", doc.RootElement.GetProperty("ssn").GetString());
    }

    [Fact]
    public async Task AuthCompleted()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new
        {
            status = "COMPLETED",
            id = "abc",
            ssn = "199001011234",
            name = "Test User",
            givenName = "Test",
            surname = "User",
        });

        var result = await client.BankIdSe.AuthAsync(new BankIdSeAuthRequest());
        var done = Assert.IsType<BankIdSeCompleted>(result);
        Assert.Equal("Test User", done.Name);
    }

    [Fact]
    public async Task AuthFailed()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new { status = "FAILED", id = "abc", error = "userCancel" });
        var result = await client.BankIdSe.AuthAsync(new BankIdSeAuthRequest());
        var failed = Assert.IsType<BankIdSeFailed>(result);
        Assert.Equal("userCancel", failed.Error);
    }

    [Fact]
    public async Task PhoneAuthPending()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new { status = "PENDING", id = "abc", hintCode = "outstandingTransaction" });

        var result = await client.BankIdSe.PhoneAuthAsync(new BankIdSePhoneAuthRequest("199001011234", "user"));

        Assert.IsType<BankIdSePendingPhone>(result);
    }

    [Fact]
    public async Task Verify()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new
        {
            ssn = "199001011234",
            name = "Test User",
            givenName = "Test",
            surname = "User",
            age = 34,
            verifiedAt = "2026-06-08T10:00:00Z",
        });

        var result = await client.BankIdSe.VerifyAsync(new BankIdSeVerifyRequest("qr"));
        Assert.Equal(34, result.Age);
        Assert.Equal("Test User", result.Name);
    }

    [Fact]
    public async Task CancelAuthSendsDelete()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.NoContent, "");
        await client.BankIdSe.CancelAuthAsync("xyz");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/v3/bankid-se/auth/xyz", handler.LastRequest.RequestUri!.AbsolutePath);
    }
}
