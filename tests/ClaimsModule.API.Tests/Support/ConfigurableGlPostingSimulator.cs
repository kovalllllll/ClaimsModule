using ClaimsModule.Domain.Reserves;
using ClaimsModule.Infrastructure.Jobs;

namespace ClaimsModule.API.Tests.Support;

public sealed class ConfigurableGlPostingSimulator : IGlPostingSimulator
{
    public bool ShouldFail { get; set; }

    public Task SimulateAsync(ReserveHistory history, CancellationToken cancellationToken)
    {
        if (ShouldFail)
            throw new InvalidOperationException("Simulated GL posting failure for integration tests.");

        return Task.CompletedTask;
    }
}
