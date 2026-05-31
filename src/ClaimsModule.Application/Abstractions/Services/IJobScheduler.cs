namespace ClaimsModule.Application.Abstractions.Services;

public interface IJobScheduler
{
    void EnqueuePostGLReserveChange(Guid reserveHistoryId, Guid claimId, string idempotencyKey);
}
