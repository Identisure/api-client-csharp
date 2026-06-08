using System.Text.Json;
using System.Text.Json.Serialization;

namespace Idkollen.Client.Models;

// --- Requests ---

public sealed record BankIdSeAuthRequest
{
    public string? Ssn { get; init; }
    public string? IpAddress { get; init; }
    public string? CallbackUrl { get; init; }
    public bool? PinRequired { get; init; }
    public string? Intent { get; init; }
    public string? OrgNumber { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record BankIdSePhoneAuthRequest(
    string Ssn,
    string CallInitiator
)
{
    public string? CallbackUrl { get; init; }
    public bool? PinRequired { get; init; }
    public string? Intent { get; init; }
    public string? OrgNumber { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record BankIdSeSignRequest(string Text)
{
    public string? Ssn { get; init; }
    public string? IpAddress { get; init; }
    public string? CallbackUrl { get; init; }
    public bool? PinRequired { get; init; }
    public string? Digest { get; init; }
    public string? OrgNumber { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record BankIdSePhoneSignRequest(
    string Ssn,
    string CallInitiator,
    string Text
)
{
    public string? CallbackUrl { get; init; }
    public bool? PinRequired { get; init; }
    public string? Digest { get; init; }
    public string? OrgNumber { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record BankIdSeVerifyRequest(string QrCode);

public sealed record BankIdSeVerifyResponse(
    string Ssn,
    string Name,
    string GivenName,
    string Surname,
    int? Age,
    string? VerifiedAt
);

// --- Status interfaces + variants ---

public interface IBankIdSeStatus { }
public interface IBankIdSePhoneStatus { }

public sealed record BankIdSePending(
    string Id,
    string? RefId,
    string? AutoStartToken,
    string? QrStartToken,
    string? QrStartSecret,
    string? HintCode
) : IBankIdSeStatus;

public sealed record BankIdSePendingPhone(
    string Id,
    string? RefId,
    string? HintCode
) : IBankIdSePhoneStatus;

public sealed record BankIdSeCompleted(
    string Id,
    string? RefId,
    string Ssn,
    string Name,
    string GivenName,
    string Surname,
    string? CertStartDate,
    string? Address,
    string? CompanySignatoryText
) : IBankIdSeStatus, IBankIdSePhoneStatus;

public sealed record BankIdSeFailed(
    string Id,
    string? RefId,
    string Error
) : IBankIdSeStatus, IBankIdSePhoneStatus;

// --- Converters ---

internal sealed class BankIdSeStatusConverter : JsonConverter<IBankIdSeStatus>
{
    public override IBankIdSeStatus Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var status = doc.RootElement.GetProperty("status").GetString();
        var raw = doc.RootElement.GetRawText();
        return status switch
        {
            "PENDING" => JsonSerializer.Deserialize<BankIdSePending>(raw, options)!,
            "COMPLETED" => JsonSerializer.Deserialize<BankIdSeCompleted>(raw, options)!,
            "FAILED" => JsonSerializer.Deserialize<BankIdSeFailed>(raw, options)!,
            _ => throw new JsonException($"Unknown bankid-se status: {status}"),
        };
    }

    public override void Write(Utf8JsonWriter w, IBankIdSeStatus v, JsonSerializerOptions o)
        => throw new NotSupportedException();
}

internal sealed class BankIdSePhoneStatusConverter : JsonConverter<IBankIdSePhoneStatus>
{
    public override IBankIdSePhoneStatus Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var status = doc.RootElement.GetProperty("status").GetString();
        var raw = doc.RootElement.GetRawText();
        return status switch
        {
            "PENDING" => JsonSerializer.Deserialize<BankIdSePendingPhone>(raw, options)!,
            "COMPLETED" => JsonSerializer.Deserialize<BankIdSeCompleted>(raw, options)!,
            "FAILED" => JsonSerializer.Deserialize<BankIdSeFailed>(raw, options)!,
            _ => throw new JsonException($"Unknown bankid-se phone status: {status}"),
        };
    }

    public override void Write(Utf8JsonWriter w, IBankIdSePhoneStatus v, JsonSerializerOptions o)
        => throw new NotSupportedException();
}
