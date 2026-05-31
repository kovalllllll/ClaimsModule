using Hangfire.Dashboard;

namespace ClaimsModule.API.Auth;

public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var role = httpContext.User.FindFirst("role")?.Value
            ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return role is "supervisor" or "manager";
    }
}
