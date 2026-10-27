using System.Security.Cryptography;
using System.Text;

namespace Identisure.Client;

public static class CallbackSignature
{
    /// <summary>
    /// Verifies the <c>X-Signature-SHA256</c> header of a callback request. Pass the raw request body as
    /// received, before parsing it.
    /// </summary>
    public static bool Verify(ReadOnlySpan<byte> body, string signature, string secret)
    {
        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), body);
        Span<byte> actual = stackalloc byte[expected.Length];

        if (!Convert.TryFromBase64String(signature, actual, out var written) || written != expected.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
