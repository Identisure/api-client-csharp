using System.Text.Json;
using System.Text.Json.Serialization;

namespace Idkollen.Client.Models;

public sealed record BankIdNoAuthRequest
{
    public string? RedirectUrl { get; init; }
    public bool? RequestSsn { get; init; }
    public bool? RequestPhone { get; init; }
    public bool? RequestEmail { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
    public string? AppCallbackUri { get; init; }
}

public sealed record BankIdNoBackchannelAuthRequest(string Ssn)
{
    public string? CallbackUrl { get; init; }
    public string? RefId { get; init; }
}

public sealed record BankIdNoSignRequest
{
    public string? RedirectUrl { get; init; }
    public string? Text { get; init; }
    public List<string>? Documents { get; init; }
    public bool? RequestSsn { get; init; }
    public bool? RequestPhone { get; init; }
    public bool? RequestEmail { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record BankIdNoSignResult(string EndUser, string Merchant, string Hash);
public sealed record BankIdNoSignedDocument(string Id, string Hash);

public interface IBankIdNoStatus { }

public sealed record BankIdNoPending(
    string Id,
    string? RefId,
    string? Url,
    string? BindingMessage
) : IBankIdNoStatus;

public sealed record BankIdNoCompleted(
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
    string? BankId,
    BankIdNoSignResult? SignResult,
    List<BankIdNoSignedDocument>? SignedDocuments
) : IBankIdNoStatus;

public sealed record BankIdNoFailed(
    string Id,
    string? RefId,
    string Error
) : IBankIdNoStatus;

internal sealed class BankIdNoStatusConverter : JsonConverter<IBankIdNoStatus>
{
    public override IBankIdNoStatus Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var status = doc.RootElement.GetProperty("status").GetString();
        var raw = doc.RootElement.GetRawText();
        return status switch
        {
            "PENDING" => JsonSerializer.Deserialize<BankIdNoPending>(raw, options)!,
            "COMPLETED" => JsonSerializer.Deserialize<BankIdNoCompleted>(raw, options)!,
            "FAILED" => JsonSerializer.Deserialize<BankIdNoFailed>(raw, options)!,
            _ => throw new JsonException($"Unknown bankid-no status: {status}"),
        };
    }

    public override void Write(Utf8JsonWriter w, IBankIdNoStatus v, JsonSerializerOptions o)
        => throw new NotSupportedException();
}
