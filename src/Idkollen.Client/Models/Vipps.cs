using System.Text.Json;
using System.Text.Json.Serialization;

namespace Idkollen.Client.Models;

public sealed record VippsAuthRequest
{
    public string? RedirectUrl { get; init; }
    public bool? RequestSsn { get; init; }
    public bool? RequestPhone { get; init; }
    public bool? RequestEmail { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
    public string? AppCallbackUri { get; init; }
}

public sealed record VippsBackchannelAuthRequest(string Phone)
{
    public bool? RequestSsn { get; init; }
    public bool? RequestEmail { get; init; }
    public bool? RequestAddress { get; init; }
    public string? CallbackUrl { get; init; }
    public string? RefId { get; init; }
}

public interface IVippsStatus { }

public sealed record VippsPending(string Id, string? RefId, string? Url) : IVippsStatus;

public sealed record VippsCompleted(
    string Id,
    string? RefId,
    string Ssn,
    string Name,
    string GivenName,
    string Surname,
    string? Phone,
    string? Email,
    string? Address,
    string? BirthDate,
    string? Pid,
    string? BankId
) : IVippsStatus;

public sealed record VippsFailed(string Id, string? RefId, string Error) : IVippsStatus;

internal sealed class VippsStatusConverter : JsonConverter<IVippsStatus>
{
    public override IVippsStatus Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var status = doc.RootElement.GetProperty("status").GetString();
        var raw = doc.RootElement.GetRawText();
        return status switch
        {
            "PENDING" => JsonSerializer.Deserialize<VippsPending>(raw, options)!,
            "COMPLETED" => JsonSerializer.Deserialize<VippsCompleted>(raw, options)!,
            "FAILED" => JsonSerializer.Deserialize<VippsFailed>(raw, options)!,
            _ => throw new JsonException($"Unknown vipps status: {status}"),
        };
    }

    public override void Write(Utf8JsonWriter w, IVippsStatus v, JsonSerializerOptions o)
        => throw new NotSupportedException();
}
