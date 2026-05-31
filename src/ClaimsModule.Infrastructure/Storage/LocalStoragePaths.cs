namespace ClaimsModule.Infrastructure.Storage;

public static class LocalStoragePaths
{
    /// <summary>
    /// Resolves LocalStorage:BasePath to an absolute directory (required by PhysicalFileProvider).
    /// Relative paths are resolved against <paramref name="contentRoot"/>.
    /// </summary>
    public static string ResolveBasePath(string? configuredPath, string contentRoot)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            return Path.Combine(contentRoot, "uploads");

        if (Path.IsPathRooted(configuredPath))
            return Path.GetFullPath(configuredPath);

        return Path.GetFullPath(Path.Combine(contentRoot, configuredPath));
    }
}
