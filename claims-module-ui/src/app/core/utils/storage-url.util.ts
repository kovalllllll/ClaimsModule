export interface StorageUrlResolveOptions {
  /**
   * Dev: ng serve proxies /uploads to the API — keep relative URLs on the SPA origin
   * to avoid mixed content and chrome-error frame issues.
   */
  useLocalUploadProxy?: boolean;
}

/**
 * Local storage returns relative signed URLs (/uploads/...?exp=&sig=).
 * In development, proxy.conf.json forwards /uploads to the API on the same origin.
 * In production, prefix the API host (Azure App Service).
 */
export function resolveStorageDownloadUrl(
  sasUrl: string | null | undefined,
  apiBaseUrl: string,
  options?: StorageUrlResolveOptions
): string {
  if (!sasUrl?.trim()) {
    return '#';
  }

  const trimmed = sasUrl.trim();
  if (/^https?:\/\//i.test(trimmed)) {
    return trimmed;
  }

  const path = trimmed.startsWith('/') ? trimmed : `/${trimmed}`;

  if (options?.useLocalUploadProxy && path.startsWith('/uploads')) {
    return path;
  }

  const base = apiBaseUrl.replace(/\/$/, '');
  return `${base}${path}`;
}

/** Resolves relative SAS URLs on document DTOs from the API. */
export function resolveDocumentDownloadUrls<T extends { sasUrl: string }>(
  documents: T[],
  apiBaseUrl: string,
  options?: StorageUrlResolveOptions
): T[] {
  return documents.map((doc) => ({
    ...doc,
    sasUrl: resolveStorageDownloadUrl(doc.sasUrl, apiBaseUrl, options),
  }));
}
