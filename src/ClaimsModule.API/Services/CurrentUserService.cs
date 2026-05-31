using System.Security.Claims;
using ClaimsModule.Application.Abstractions.Services;

namespace ClaimsModule.API.Services;

internal sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var claim = accessor.HttpContext?.User?.FindFirst("sub")
                ?? accessor.HttpContext?.User?.FindFirst("userId")
                ?? accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public string? UserName =>
        accessor.HttpContext?.User?.FindFirst("name")?.Value
        ?? accessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Role =>
        accessor.HttpContext?.User?.FindFirst("role")?.Value
        ?? accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAuthenticated =>
        accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
