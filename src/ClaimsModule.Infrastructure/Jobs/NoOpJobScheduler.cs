using ClaimsModule.Application.Abstractions.Services;

namespace ClaimsModule.Infrastructure.Jobs;

public sealed class NoOpJobScheduler : IJobScheduler
{
    public void EnqueuePostGLReserveChange(Guid reserveHistoryId, Guid claimId, string idempotencyKey)
    {
    }
}
