using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Idkollen.Client.Models;
using Xunit;

namespace Idkollen.Client.Tests;

public class FixtureTests
{
    private static readonly JsonSerializerOptions CamelCase = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly PollOptions FastPoll = new(TimeSpan.FromMilliseconds(1));
    private static readonly string ExpectedAuth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("cid:sec"));

    private sealed record Flow(
        string Dir,
        string Segment,
        Type Pending,
        Type Completed,
        Type Failed,
        Type Unknown,
        Func<IdkollenClient, Task<object>> StartAuth,
        Func<IdkollenClient, string, Task<object>> WaitForAuth,
        Func<IdkollenClient, Task<object>>? StartSign = null,
        Func<IdkollenClient, string, Task<object>>? WaitForSign = null);

    private static readonly Dictionary<string, Flow> Flows = new()
    {
        ["bankid_se"] = new(
            "bankid_se", "bankid-se",
            typeof(BankIdSePending), typeof(BankIdSeCompleted), typeof(BankIdSeFailed), typeof(BankIdSeUnknown),
            async c => await c.BankIdSe.AuthAsync(new BankIdSeAuthRequest()),
            async (c, id) => await c.BankIdSe.WaitForAuthAsync(id, FastPoll),
            async c => await c.BankIdSe.SignAsync(new BankIdSeSignRequest("Sign this")),
            async (c, id) => await c.BankIdSe.WaitForSignAsync(id, FastPoll)),
        ["bankid_no"] = new(
            "bankid_no", "bankid-no",
            typeof(BankIdNoPending), typeof(BankIdNoCompleted), typeof(BankIdNoFailed), typeof(BankIdNoUnknown),
            async c => await c.BankIdNo.AuthAsync(new BankIdNoAuthRequest()),
            async (c, id) => await c.BankIdNo.WaitForAuthAsync(id, FastPoll),
            async c => await c.BankIdNo.SignAsync(new BankIdNoSignRequest { Text = "Sign this" }),
            async (c, id) => await c.BankIdNo.WaitForSignAsync(id, FastPoll)),
        ["freja"] = new(
            "freja", "freja",
            typeof(FrejaPending), typeof(FrejaCompleted), typeof(FrejaFailed), typeof(FrejaUnknown),
            async c => await c.Freja.AuthAsync(new FrejaAuthRequest()),
            async (c, id) => await c.Freja.WaitForAuthAsync(id, FastPoll),
            async c => await c.Freja.SignAsync(new FrejaSignRequest("Sign this")),
            async (c, id) => await c.Freja.WaitForSignAsync(id, FastPoll)),
        ["mitid"] = new(
            "mitid", "mitid",
            typeof(MitIdPending), typeof(MitIdCompleted), typeof(MitIdFailed), typeof(MitIdUnknown),
            async c => await c.MitId.AuthAsync(new MitIdAuthRequest()),
            async (c, id) => await c.MitId.WaitForAuthAsync(id, FastPoll),
            async c => await c.MitId.SignAsync(new MitIdSignRequest("Sign this")),
            async (c, id) => await c.MitId.WaitForSignAsync(id, FastPoll)),
        ["ftn"] = new(
            "ftn", "ftn",
            typeof(FtnPending), typeof(FtnCompleted), typeof(FtnFailed), typeof(FtnUnknown),
            async c => await c.Ftn.AuthAsync(new FtnAuthRequest()),
            async (c, id) => await c.Ftn.WaitForAuthAsync(id, FastPoll)),
        ["vipps"] = new(
            "vipps", "vipps",
            typeof(VippsPending), typeof(VippsCompleted), typeof(VippsFailed), typeof(VippsUnknown),
            async c => await c.Vipps.AuthAsync(new VippsAuthRequest()),
            async (c, id) => await c.Vipps.WaitForAuthAsync(id, FastPoll)),
    };

    public static TheoryData<string> AllProviders => new() { "bankid_se", "bankid_no", "freja", "mitid", "ftn", "vipps" };
    public static TheoryData<string> SignProviders => new() { "bankid_se", "bankid_no", "freja", "mitid" };

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task AuthCompletes(string provider)
    {
        var flow = Flows[provider];
        var (client, handler) = Build();
        var id = Id(flow.Dir + "/auth_pending.json");
        handler.Enqueue(HttpStatusCode.Created, Fixture(flow.Dir + "/auth_pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture(flow.Dir + "/auth_pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture(flow.Dir + "/auth_completed.json"));

        var pending = await flow.StartAuth(client);
        Assert.IsType(flow.Pending, pending);
        AssertMatches(pending, flow.Dir + "/auth_pending.json");

        var result = await flow.WaitForAuth(client, id);
        Assert.IsType(flow.Completed, result);
        AssertMatches(result, flow.Dir + "/auth_completed.json");

        AssertRequests(handler,
            ("POST", $"/v3/{flow.Segment}/auth"),
            ("GET", $"/v3/{flow.Segment}/auth/{id}"),
            ("GET", $"/v3/{flow.Segment}/auth/{id}"));
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task AuthFails(string provider)
    {
        var flow = Flows[provider];
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.Created, Fixture(flow.Dir + "/auth_pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture(flow.Dir + "/auth_failed.json"));

        await flow.StartAuth(client);
        var result = await flow.WaitForAuth(client, Id(flow.Dir + "/auth_pending.json"));

        Assert.IsType(flow.Failed, result);
        AssertMatches(result, flow.Dir + "/auth_failed.json");
        Assert.Equal("CANCELLED", Actual(result)["error"]!.GetValue<string>());
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task AuthUnknownStatus(string provider)
    {
        var flow = Flows[provider];
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.Created, Fixture(flow.Dir + "/auth_pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture(flow.Dir + "/auth_unknown.json"));

        await flow.StartAuth(client);
        var result = await flow.WaitForAuth(client, Id(flow.Dir + "/auth_pending.json"));

        Assert.IsType(flow.Unknown, result);
        Assert.Equal("SOMETHING_NEW", Actual(result)["status"]!.GetValue<string>());
    }

    [Theory]
    [MemberData(nameof(SignProviders))]
    public async Task SignCompletes(string provider)
    {
        var flow = Flows[provider];
        var (client, handler) = Build();
        var id = Id(flow.Dir + "/auth_pending.json");
        handler.Enqueue(HttpStatusCode.Created, Fixture(flow.Dir + "/auth_pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture(flow.Dir + "/auth_pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture(flow.Dir + "/sign_completed.json"));

        var pending = await flow.StartSign!(client);
        Assert.IsType(flow.Pending, pending);

        var result = await flow.WaitForSign!(client, id);
        Assert.IsType(flow.Completed, result);
        AssertMatches(result, flow.Dir + "/sign_completed.json");

        AssertRequests(handler,
            ("POST", $"/v3/{flow.Segment}/sign"),
            ("GET", $"/v3/{flow.Segment}/sign/{id}"),
            ("GET", $"/v3/{flow.Segment}/sign/{id}"));
    }

    [Fact]
    public async Task BankIdSePhoneAuthCompletes()
    {
        var (client, handler) = Build();
        var id = Id("bankid_se/phone_auth_pending.json");
        handler.Enqueue(HttpStatusCode.Created, Fixture("bankid_se/phone_auth_pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture("bankid_se/auth_completed.json"));

        var pending = Assert.IsType<BankIdSePendingPhone>(
            await client.BankIdSe.PhoneAuthAsync(new BankIdSePhoneAuthRequest("199001011239", "USER")));
        Assert.Equal(id, pending.Id);
        Assert.Null(pending.HintCode);

        var result = await client.BankIdSe.WaitForAuthAsync(id, FastPoll);
        Assert.IsType<BankIdSeCompleted>(result);
        AssertMatches(result, "bankid_se/auth_completed.json");

        AssertRequests(handler,
            ("POST", "/v3/bankid-se/phone/auth"),
            ("GET", $"/v3/bankid-se/auth/{id}"));
    }

    [Fact]
    public async Task BankIdSeVerify()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, Fixture("bankid_se/verify.json"));

        var result = await client.BankIdSe.VerifyAsync(new BankIdSeVerifyRequest("bankidqr.1.2.3"));

        AssertMatches(result, "bankid_se/verify.json");
        AssertRequests(handler, ("POST", "/v3/bankid-se/verify"));
    }

    [Fact]
    public async Task AgeVerificationCompletes()
    {
        var (client, handler) = Build();
        var id = Id("age_verification/pending.json");
        handler.Enqueue(HttpStatusCode.Created, Fixture("age_verification/pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture("age_verification/pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture("age_verification/completed.json"));

        var pending = await client.BankIdSe.AgeVerificationAsync(new AgeVerificationRequest { MinAge = 18 });
        Assert.IsType<AgeVerificationPending>(pending);
        AssertMatches(pending, "age_verification/pending.json");

        var result = Assert.IsType<AgeVerificationCompleted>(await client.BankIdSe.WaitForAgeVerificationAsync(id, FastPoll));
        Assert.True(result.AgeVerified);
        AssertMatches(result, "age_verification/completed.json");

        AssertRequests(handler,
            ("POST", "/v3/bankid-se/age-verification"),
            ("GET", $"/v3/bankid-se/age-verification/{id}"),
            ("GET", $"/v3/bankid-se/age-verification/{id}"));
    }

    [Fact]
    public async Task AgeVerificationFails()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.Created, Fixture("age_verification/pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture("age_verification/failed.json"));

        await client.BankIdSe.AgeVerificationAsync(new AgeVerificationRequest { MinAge = 18 });
        var result = await client.BankIdSe.WaitForAgeVerificationAsync(Id("age_verification/pending.json"), FastPoll);

        Assert.IsType<AgeVerificationFailed>(result);
        AssertMatches(result, "age_verification/failed.json");
    }

    [Fact]
    public async Task AgeVerificationUnknownStatus()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.Created, Fixture("age_verification/pending.json"));
        handler.Enqueue(HttpStatusCode.OK, Fixture("age_verification/unknown.json"));

        await client.BankIdSe.AgeVerificationAsync(new AgeVerificationRequest { MinAge = 18 });
        var result = await client.BankIdSe.WaitForAgeVerificationAsync(Id("age_verification/pending.json"), FastPoll);

        var unknown = Assert.IsType<AgeVerificationUnknown>(result);
        Assert.Equal("SOMETHING_NEW", unknown.Status);
    }

    [Fact]
    public async Task DocumentUpload()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.Created, Fixture("document/upload.json"));

        var result = await client.Document.UploadAsync(Encoding.UTF8.GetBytes("%PDF-1.7"), "contract.pdf");

        AssertMatches(result, "document/upload.json");
        AssertRequests(handler, ("POST", "/v3/document"));
    }

    [Fact]
    public async Task ErrorResponse()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.BadRequest, Fixture("error.json"));

        var ex = await Assert.ThrowsAsync<IdkollenException>(() => client.Ftn.AuthAsync(new FtnAuthRequest()));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("Invalid request", ex.Message);
    }

    private static (IdkollenClient Client, MockHttpMessageHandler Handler) Build()
    {
        var handler = new MockHttpMessageHandler();
        var client = new IdkollenClientBuilder("cid", "sec")
            .BaseUrl("https://x.test")
            .HttpClient(new HttpClient(handler))
            .Build();
        return (client, handler);
    }

    private static string Fixture(string path)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", path));

    private static string Id(string path)
        => JsonNode.Parse(Fixture(path))!["id"]!.GetValue<string>();

    private static JsonObject Actual(object result)
        => JsonSerializer.SerializeToNode(result, result.GetType(), CamelCase)!.AsObject();

    private static void AssertMatches(object result, string path)
    {
        var expected = JsonNode.Parse(Fixture(path))!.AsObject();
        var actual = Actual(result);
        foreach (var (key, value) in expected)
        {
            if (key == "status")
            {
                continue;
            }

            Assert.True(actual.ContainsKey(key), $"{result.GetType().Name} has no {key}");
            Assert.True(JsonNode.DeepEquals(value, actual[key]), $"{key}: expected {value?.ToJsonString()}, got {actual[key]?.ToJsonString()}");
        }
    }

    private static void AssertRequests(MockHttpMessageHandler handler, params (string Method, string Path)[] expected)
    {
        Assert.Equal(expected, handler.Requests.Select(r => (r.Method.Method, r.RequestUri!.AbsolutePath)).ToArray());
        foreach (var request in handler.Requests)
        {
            Assert.Equal(ExpectedAuth, request.Headers.GetValues("Authorization").Single());
        }
    }
}
