namespace ClaimsModule.Application.Abstractions.Services;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
