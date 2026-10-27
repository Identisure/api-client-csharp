using System.Text;
using Xunit;

namespace Identisure.Client.Tests;

public class CallbackSignatureTests
{
    private const string Secret = "test-secret";
    private const string Body = "{\"id\":\"abc\",\"status\":\"COMPLETED\"}";
    private const string Signature = "Ag2m3etnW31OP585jvcrvOupK7Vgsrzcq17KxKvkIOs=";

    [Fact]
    public void ValidSignature()
    {
        Assert.True(CallbackSignature.Verify(Encoding.UTF8.GetBytes(Body), Signature, Secret));
    }

    [Fact]
    public void TamperedBody()
    {
        Assert.False(CallbackSignature.Verify(Encoding.UTF8.GetBytes(Body + " "), Signature, Secret));
    }

    [Fact]
    public void MalformedSignature()
    {
        Assert.False(CallbackSignature.Verify(Encoding.UTF8.GetBytes(Body), "not base64!", Secret));
    }
}
