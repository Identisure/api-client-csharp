using System.Text.Json;
using System.Text.Json.Serialization;

namespace Idkollen.Client.Models;

public sealed record MitIdAuthRequest
{
    public string? RedirectUrl { get; init; }
    public string? ReferenceText { get; init; }
    public bool? RequestPhone { get; init; }
    public bool? RequestEmail { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public sealed record MitIdBackchannelAuthRequest(string Ssn, string BindingMessage)
{
    public string? CallbackUrl { get; init; }
    public string? RefId { get; init; }
}

public sealed record MitIdSignRequest(string Text)
{
    public string? RedirectUrl { get; init; }
    public string? RefId { get; init; }
}

public sealed record MitIdSignResult(string Checksum);

public interface IMitIdStatus { }

public sealed record MitIdPending(
    string Id,
    string? RefId,
    string? Url,
    string? BindingMessage
) : IMitIdStatus;

public sealed record MitIdCompleted(
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
    MitIdSignResult? SignResult
) : IMitIdStatus;

public sealed record MitIdFailed(string Id, string? RefId, string Error) : IMitIdStatus;

internal sealed class MitIdStatusConverter : JsonConverter<IMitIdStatus>
{
    public override IMitIdStatus Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var status = doc.RootElement.GetProperty("status").GetString();
        var raw = doc.RootElement.GetRawText();
        return status switch
        {
            "PENDING" => JsonSerializer.Deserialize<MitIdPending>(raw, options)!,
            "COMPLETED" => JsonSerializer.Deserialize<MitIdCompleted>(raw, options)!,
            "FAILED" => JsonSerializer.Deserialize<MitIdFailed>(raw, options)!,
            _ => throw new JsonException($"Unknown mitid status: {status}"),
        };
    }

    public override void Write(Utf8JsonWriter w, IMitIdStatus v, JsonSerializerOptions o)
        => throw new NotSupportedException();
}
