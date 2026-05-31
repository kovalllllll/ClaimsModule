using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common;

namespace ClaimsModule.Infrastructure.Storage;

public sealed class LocalFileSystemStorageService : IStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly string _signingKey;

    public LocalFileSystemStorageService(string basePath, string baseUrl, string signingKey)
    {
        _basePath = basePath;
        _baseUrl = baseUrl.TrimEnd('/');
        _signingKey = signingKey;
    }

    public async Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, blobPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file, ct);

        return $"{_baseUrl}/{blobPath}";
    }

    public Task<string> GetSasUrlAsync(string blobPath, TimeSpan ttl, CancellationToken ct = default)
    {
        var expires = DateTimeOffset.UtcNow.Add(ttl).ToUnixTimeSeconds();
        var sig = LocalStorageUrlSigner.Sign(blobPath, expires, _signingKey);
        var url = $"{_baseUrl}/{blobPath}?exp={expires}&sig={sig}";
        return Task.FromResult(url);
    }

    public Task DeleteAsync(string blobPath, CancellationToken ct = default)
    {
        var fullPath = LocalStorageFileLocator.FindExistingFile(_basePath, blobPath);
        if (fullPath is not null)
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
