using System.Security.Cryptography;
using System.Text;

namespace ClaimsModule.Infrastructure.Storage;

public static class LocalStorageUrlSigner
{
    public static string Sign(string blobPath, long expiresUnix, string signingKey)
    {
        var payload = $"{blobPath}|{expiresUnix}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static bool TryValidate(string blobPath, long expiresUnix, string signature, string signingKey)
    {
        if (expiresUnix <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            return false;

        var expected = Sign(blobPath, expiresUnix, signingKey);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }
}
