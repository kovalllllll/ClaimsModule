using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ClaimsModule.Application.Abstractions.Services;

namespace ClaimsModule.Infrastructure.Storage;

public sealed class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient, string containerName)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
    }

    public async Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken ct = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = container.GetBlobClient(blobPath);
        await blob.UploadAsync(content, overwrite: true, cancellationToken: ct);
        return blob.Uri.ToString();
    }

    public Task<string> GetSasUrlAsync(string blobPath, TimeSpan ttl, CancellationToken ct = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blob = container.GetBlobClient(blobPath);

        var sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(ttl));
        return Task.FromResult(sasUri.ToString());
    }

    public async Task DeleteAsync(string blobPath, CancellationToken ct = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blob = container.GetBlobClient(blobPath);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }
}
