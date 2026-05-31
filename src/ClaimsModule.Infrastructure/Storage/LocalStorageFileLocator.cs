using ClaimsModule.Application.Common;

namespace ClaimsModule.Infrastructure.Storage;

internal static class LocalStorageFileLocator
{
    private const string LegacyContainerPrefix = "claim-documents/";

    public static string? FindExistingFile(string basePath, string storedBlobPath)
    {
        var normalized = StorageBlobPathNormalizer.ResolveReadPath(storedBlobPath);
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ToFullPath(basePath, normalized),
            ToFullPath(basePath, storedBlobPath),
            ToFullPath(basePath, $"{LegacyContainerPrefix}{normalized}"),
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return SearchClaimFolder(basePath, normalized);
    }

    private static string? SearchClaimFolder(string basePath, string normalizedBlobPath)
    {
        var segments = normalizedBlobPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length < 3)
            return null;

        var claimDir = Path.Combine(basePath, segments[0], segments[1]);
        if (!Directory.Exists(claimDir))
            return null;

        var expectedFileName = segments[^1];
        var exact = Path.Combine(claimDir, expectedFileName);
        if (File.Exists(exact))
            return exact;

        return Directory
            .EnumerateFiles(claimDir)
            .FirstOrDefault(path =>
                string.Equals(Path.GetFileName(path), expectedFileName, StringComparison.OrdinalIgnoreCase));
    }

    private static string ToFullPath(string basePath, string blobPath)
        => Path.Combine(basePath, blobPath.Replace('/', Path.DirectorySeparatorChar));
}
