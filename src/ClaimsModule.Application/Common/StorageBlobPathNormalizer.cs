namespace ClaimsModule.Application.Common;

/// <summary>
/// Resolves stored blob paths for read/SAS operations across legacy and current upload layouts.
/// </summary>
public static class StorageBlobPathNormalizer
{
    private const string LegacyContainerPrefix = "claim-documents/";

    /// <summary>
    /// Returns the blob name for storage providers. Legacy rows stored
    /// <c>claim-documents/{orgId}/{claimId}/{file}</c>; current rows store
    /// <c>{orgId}/{claimId}/{file}</c> (container name is configured separately).
    /// </summary>
    public static string ResolveReadPath(string storedPath)
    {
        if (storedPath.StartsWith(LegacyContainerPrefix, StringComparison.OrdinalIgnoreCase))
            return storedPath[LegacyContainerPrefix.Length..];

        return storedPath;
    }
}
