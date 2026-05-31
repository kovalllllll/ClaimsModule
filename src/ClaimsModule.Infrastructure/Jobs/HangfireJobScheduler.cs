using ClaimsModule.Application.Abstractions.Services;
using Hangfire;

namespace ClaimsModule.Infrastructure.Jobs;

public sealed class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _client;

    public HangfireJobScheduler(IBackgroundJobClient client) => _client = client;

    public void EnqueuePostGLReserveChange(Guid reserveHistoryId, Guid claimId, string idempotencyKey)
        => _client.Enqueue<PostGLReserveChangeJob>(job =>
            job.ExecuteAsync(reserveHistoryId, claimId, idempotencyKey, null));
}
