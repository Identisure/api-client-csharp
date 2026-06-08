using Idkollen.Client.Models;

namespace Idkollen.Client.Endpoints;

public sealed class DocumentEndpoint
{
    private readonly Transport _transport;
    internal DocumentEndpoint(Transport transport) { _transport = transport; }

    public Task<DocumentUploadResponse> UploadAsync(byte[] data, string filename, string mimeType = "application/pdf", CancellationToken ct = default)
        => _transport.PostMultipartAsync<DocumentUploadResponse>("/document", data, filename, mimeType, ct);

    public Task<byte[]> DownloadAsync(string id, CancellationToken ct = default)
        => _transport.GetRawAsync("/document/" + id, ct);

    public Task DeleteAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/document/" + id, ct);
}
