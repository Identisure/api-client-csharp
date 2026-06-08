using System.Text.Json;
using System.Text.Json.Serialization;

namespace Idkollen.Client.Models;

public sealed record FrejaAuthRequest
{
    public string? Ssn { get; init; }
    public string? CallbackUrl { get; init; }
    public string? MinRegistrationLevel { get; init; }
    public string? OrgNumber { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record FrejaBackchannelAuthRequest(string Ssn, string Country)
{
    public string? CallbackUrl { get; init; }
    public string? MinRegistrationLevel { get; init; }
    public string? OrgNumber { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record FrejaSignRequest(string Text)
{
    public string? Ssn { get; init; }
    public string? CallbackUrl { get; init; }
    public string? MinRegistrationLevel { get; init; }
    public string? OrgNumber { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record FrejaBackchannelSignRequest(string Ssn, string Country, string Text)
{
    public string? CallbackUrl { get; init; }
    public string? MinRegistrationLevel { get; init; }
    public string? OrgNumber { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public interface IFrejaStatus { }

public sealed record FrejaPending(
    string Id,
    string? RefId,
    string AutoStartToken,
    string QrData
) : IFrejaStatus;

public sealed record FrejaCompleted(
    string Id,
    string? RefId,
    string Ssn,
    string Country,
    string Name,
    string GivenName,
    string Surname,
    string? Address,
    string? CompanySignatoryText
) : IFrejaStatus;

public sealed record FrejaFailed(string Id, string? RefId, string Error) : IFrejaStatus;

internal sealed class FrejaStatusConverter : JsonConverter<IFrejaStatus>
{
    public override IFrejaStatus Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var status = doc.RootElement.GetProperty("status").GetString();
        var raw = doc.RootElement.GetRawText();
        return status switch
        {
            "PENDING" => JsonSerializer.Deserialize<FrejaPending>(raw, options)!,
            "COMPLETED" => JsonSerializer.Deserialize<FrejaCompleted>(raw, options)!,
            "FAILED" => JsonSerializer.Deserialize<FrejaFailed>(raw, options)!,
            _ => throw new JsonException($"Unknown freja status: {status}"),
        };
    }

    public override void Write(Utf8JsonWriter w, IFrejaStatus v, JsonSerializerOptions o)
        => throw new NotSupportedException();
}
