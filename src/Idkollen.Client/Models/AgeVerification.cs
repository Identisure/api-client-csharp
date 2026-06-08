using System.Text.Json;
using System.Text.Json.Serialization;

namespace Idkollen.Client.Models;

public sealed record AgeVerificationRequest
{
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public string? RefId { get; init; }
    public string? CallbackUrl { get; init; }
    public string? RedirectUrl { get; init; }
}

public interface IAgeVerificationStatus { }

public sealed record AgeVerificationPending(
    string Id,
    string? Url,
    int? MinAge,
    int? MaxAge
) : IAgeVerificationStatus;

public sealed record AgeVerificationCompleted(
    string Id,
    bool AgeVerified
) : IAgeVerificationStatus;

public sealed record AgeVerificationFailed(
    string Id,
    string Error
) : IAgeVerificationStatus;

internal sealed class AgeVerificationStatusConverter : JsonConverter<IAgeVerificationStatus>
{
    public override IAgeVerificationStatus Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var status = doc.RootElement.GetProperty("status").GetString();
        var raw = doc.RootElement.GetRawText();
        return status switch
        {
            "PENDING" => JsonSerializer.Deserialize<AgeVerificationPending>(raw, options)!,
            "COMPLETED" => JsonSerializer.Deserialize<AgeVerificationCompleted>(raw, options)!,
            "FAILED" => JsonSerializer.Deserialize<AgeVerificationFailed>(raw, options)!,
            _ => throw new JsonException($"Unknown age verification status: {status}"),
        };
    }

    public override void Write(Utf8JsonWriter w, IAgeVerificationStatus v, JsonSerializerOptions o)
        => throw new NotSupportedException("AgeVerification status is read-only");
}
