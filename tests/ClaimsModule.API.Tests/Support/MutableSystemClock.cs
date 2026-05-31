using ClaimsModule.Application.Abstractions.Services;

namespace ClaimsModule.API.Tests.Support;

public sealed class MutableSystemClock : ISystemClock
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
}
