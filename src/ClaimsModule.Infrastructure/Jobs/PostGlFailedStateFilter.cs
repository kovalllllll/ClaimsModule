using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Jobs;

public sealed class PostGlFailedStateFilter : IElectStateFilter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PostGlFailedStateFilter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState is not FailedState)
            return;

        if (context.BackgroundJob?.Job?.Type != typeof(PostGLReserveChangeJob))
            return;

        if (context.BackgroundJob.Job.Args.Count < 3 ||
            context.BackgroundJob.Job.Args[0] is not Guid reserveHistoryId)
            return;

        var failureReason = context.CandidateState is FailedState failed
            ? failed.Reason ?? "GL posting job failed after all retries."
            : "GL posting job failed after all retries.";

        GlPostingFailureApplier.Apply(_scopeFactory, reserveHistoryId, failureReason);
    }

    public void OnStateUnapplied(ElectStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}
