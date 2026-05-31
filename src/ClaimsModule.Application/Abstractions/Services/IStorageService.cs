namespace ClaimsModule.Application.Abstractions.Services;

public interface IStorageService
{
    Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken ct = default);
    Task<string> GetSasUrlAsync(string blobPath, TimeSpan ttl, CancellationToken ct = default);
    Task DeleteAsync(string blobPath, CancellationToken ct = default);
}
