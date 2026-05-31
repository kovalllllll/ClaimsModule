using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.RetryGlPosting;

public sealed class RetryGlPostingCommandHandler(
    IReserveRepository reserves,
    IUnitOfWork unitOfWork,
    IJobScheduler jobScheduler,
    IAuditLogService auditLog)
    : IRequestHandler<RetryGlPostingCommand, Unit>
{
    public async Task<Unit> Handle(RetryGlPostingCommand request, CancellationToken cancellationToken)
    {
        var history = await reserves.GetHistoryByIdAsync(
                request.ReserveHistoryId, request.ClaimId, cancellationToken)
            ?? throw new KeyNotFoundException($"Reserve history {request.ReserveHistoryId} not found.");

        if (history.OrganisationId != request.OrganisationId
            || history.PostingStatus != ReservePostingStatus.Failed)
        {
            throw new KeyNotFoundException($"Reserve history {request.ReserveHistoryId} not found.");
        }

        history.ResetPostingForRetry();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        jobScheduler.EnqueuePostGLReserveChange(
            history.Id,
            history.ClaimId,
            history.IdempotencyKey.Value);

        await auditLog.WriteAsync(
            claimId: request.ClaimId,
            eventType: AuditEventType.GlPostingSimulated,
            description: $"GL posting retry enqueued for reserve history {history.Id}.",
            relatedEntityId: history.Id,
            relatedEntityType: "ReserveHistory",
            ct: cancellationToken);

        return Unit.Value;
    }
}
