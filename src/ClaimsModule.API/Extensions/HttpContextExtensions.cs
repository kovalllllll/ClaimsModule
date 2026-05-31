using ClaimsModule.API.Middleware;
using ClaimsModule.API.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.Extensions;

internal static class HttpContextExtensions
{
    public static Guid GetOrganisationId(this HttpContext context)
    {
        var claim = context.User.FindFirst("organisationId")?.Value;
        if (Guid.TryParse(claim, out var id))
            return id;

        var tenant = context.RequestServices.GetService<IOptions<TenantOptions>>()?.Value;
        return tenant?.DefaultOrganisationId
               ?? throw new InvalidOperationException(
                   "Tenant:DefaultOrganisationId is not configured.");
    }

    public static string? GetIdempotencyKey(this HttpContext context) =>
        context.Items.TryGetValue(IdempotencyKeyMiddleware.ItemKey, out var value)
            ? value as string
            : null;

    public static Guid? GetCorrelationId(this HttpContext context) =>
        context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var value) && value is Guid id
            ? id
            : null;

    public static bool TryGetRequiredIdempotencyKey(
        this HttpContext context,
        out string idempotencyKey,
        out UnprocessableEntityObjectResult? errorResult)
    {
        idempotencyKey = context.GetIdempotencyKey() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            errorResult = null;
            return true;
        }

        errorResult = new UnprocessableEntityObjectResult(new
        {
            type = "ValidationError",
            title = "One or more validation errors occurred.",
            status = 422,
            errors = new Dictionary<string, string[]>
            {
                ["Idempotency-Key"] = ["Idempotency-Key header is required for this operation."]
            }
        });
        return false;
    }
}
