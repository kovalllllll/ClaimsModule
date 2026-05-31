namespace ClaimsModule.API.Middleware;

public sealed class IdempotencyKeyMiddleware(RequestDelegate next)
{
    public const string ItemKey = "IdempotencyKey";
    public const string HeaderName = "Idempotency-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        var key = context.Request.Headers[HeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(key))
        {
            context.Items[ItemKey] = key;
        }

        await next(context);
    }
}
