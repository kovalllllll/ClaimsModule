using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.API.Tests.Support;

public sealed class SynchronousJobScheduler : IJobScheduler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SynchronousJobScheduler(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public void EnqueuePostGLReserveChange(Guid reserveHistoryId, Guid claimId, string idempotencyKey)
    {
        using var scope = _scopeFactory.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<PostGLReserveChangeJob>();
        job.ExecuteAsync(reserveHistoryId, claimId, idempotencyKey, null).GetAwaiter().GetResult();
    }
}
