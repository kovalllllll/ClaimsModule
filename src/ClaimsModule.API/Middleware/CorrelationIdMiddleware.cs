namespace ClaimsModule.API.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string ItemKey = "CorrelationId";
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.TryParse(context.Request.Headers[HeaderName].FirstOrDefault(), out var parsed)
            ? parsed
            : Guid.NewGuid();

        context.Items[ItemKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId.ToString();

        await next(context);
    }
}
