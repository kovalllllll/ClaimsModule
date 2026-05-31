using ClaimsModule.Application.Abstractions.Services;

namespace ClaimsModule.API.Services;

internal sealed class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
