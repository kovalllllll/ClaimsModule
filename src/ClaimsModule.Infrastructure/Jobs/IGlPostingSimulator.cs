using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Infrastructure.Jobs;

public interface IGlPostingSimulator
{
    Task SimulateAsync(ReserveHistory history, CancellationToken cancellationToken);
}

public sealed class DefaultGlPostingSimulator : IGlPostingSimulator
{
    public Task SimulateAsync(ReserveHistory history, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
