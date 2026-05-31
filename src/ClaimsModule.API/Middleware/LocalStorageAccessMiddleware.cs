using ClaimsModule.Infrastructure.Storage;

namespace ClaimsModule.API.Middleware;

public sealed class LocalStorageAccessMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly string _signingKey = configuration["LocalStorage:SigningKey"]
                                          ?? "ClaimsModuleLocalStorageSigningKeyDevOnly2026!";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/uploads", out var remaining))
        {
            await next(context);
            return;
        }

        var remainingPath = remaining.Value ?? string.Empty;
        var blobPath = remainingPath.TrimStart('/').Replace('\\', '/');
        if (string.IsNullOrEmpty(blobPath))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (!context.Request.Query.TryGetValue("exp", out var expValues)
            || !long.TryParse(expValues.FirstOrDefault(), out var expiresUnix)
            || !context.Request.Query.TryGetValue("sig", out var sigValues)
            || string.IsNullOrWhiteSpace(sigValues.FirstOrDefault()))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (!LocalStorageUrlSigner.TryValidate(blobPath, expiresUnix, sigValues.First()!, _signingKey))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }
}