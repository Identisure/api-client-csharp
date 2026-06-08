using System.Net;
using System.Text;
using Idkollen.Client.Models;
using Xunit;

namespace Idkollen.Client.Tests;

public class DocumentTests
{
    private static (Idkollen.Client.IdkollenClient Client, MockHttpMessageHandler Handler) Build()
    {
        var handler = new MockHttpMessageHandler();
        var http = new HttpClient(handler);
        var client = new Idkollen.Client.IdkollenClientBuilder("cid", "sec")
            .BaseUrl("https://x.test").HttpClient(http).Build();
        return (client, handler);
    }

    [Fact]
    public async Task Upload()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, new { id = "doc1", hash = "h1" });
        var data = Encoding.UTF8.GetBytes("PDF-DATA");
        var result = await client.Document.UploadAsync(data, "contract.pdf");
        Assert.Equal("doc1", result.Id);
        Assert.Equal("h1", result.Hash);

        var req = handler.LastRequest;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.StartsWith("multipart/form-data; boundary=", req.Content!.Headers.ContentType!.ToString());
        Assert.Contains("contract.pdf", handler.LastRequestBody);
        Assert.Contains("PDF-DATA", handler.LastRequestBody);
    }

    [Fact]
    public async Task DownloadReturnsRawBytes()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.OK, "\x25PDF-binary-bytes");
        var result = await client.Document.DownloadAsync("doc1");
        Assert.Equal("\x25PDF-binary-bytes", Encoding.UTF8.GetString(result));
    }

    [Fact]
    public async Task Delete()
    {
        var (client, handler) = Build();
        handler.Enqueue(HttpStatusCode.NoContent, "");
        await client.Document.DeleteAsync("doc1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/document/doc1", handler.LastRequest.RequestUri!.AbsolutePath);
    }
}
