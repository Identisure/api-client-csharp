using System.Text.Json;
using System.Text.Json.Serialization;

namespace Idkollen.Client.Models;

public sealed record FtnAuthRequest
{
    public string? RedirectUrl { get; init; }
    public bool? RequestPhone { get; init; }
    public bool? RequestEmail { get; init; }
    public bool? RequestAddress { get; init; }
    public string? RefId { get; init; }
}

public interface IFtnStatus { }

public sealed record FtnPending(string Id, string? RefId, string Url) : IFtnStatus;

public sealed record FtnCompleted(
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
) : IFtnStatus;

public sealed record FtnFailed(string Id, string? RefId, string Error) : IFtnStatus;

internal sealed class FtnStatusConverter : JsonConverter<IFtnStatus>
{
    public override IFtnStatus Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var status = doc.RootElement.GetProperty("status").GetString();
        var raw = doc.RootElement.GetRawText();
        return status switch
        {
            "PENDING" => JsonSerializer.Deserialize<FtnPending>(raw, options)!,
            "COMPLETED" => JsonSerializer.Deserialize<FtnCompleted>(raw, options)!,
            "FAILED" => JsonSerializer.Deserialize<FtnFailed>(raw, options)!,
            _ => throw new JsonException($"Unknown ftn status: {status}"),
        };
    }

    public override void Write(Utf8JsonWriter w, IFtnStatus v, JsonSerializerOptions o)
        => throw new NotSupportedException();
}
