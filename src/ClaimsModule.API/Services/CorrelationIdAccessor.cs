using ClaimsModule.API.Middleware;
using ClaimsModule.Application.Abstractions.Services;

namespace ClaimsModule.API.Services;

internal sealed class CorrelationIdAccessor(IHttpContextAccessor accessor) : ICorrelationIdAccessor
{
    public Guid? CorrelationId
    {
        get
        {
            var context = accessor.HttpContext;
            if (context?.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var value) == true
                && value is Guid id)
            {
                return id;
            }

            return null;
        }
    }
}
