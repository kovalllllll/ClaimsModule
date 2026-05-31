using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Domain.Enums;
using Hangfire;
using Hangfire.Server;

namespace ClaimsModule.Infrastructure.Jobs;

public sealed class PostGLReserveChangeJob
{
    private const int MaxRetryAttempts = 3;

    private readonly IReserveRepository _reserves;
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly IGlPostingSimulator _simulator;

    public PostGLReserveChangeJob(
        IReserveRepository reserves,
        IClaimRepository claims,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog,
        IGlPostingSimulator simulator)
    {
        _reserves = reserves;
        _claims = claims;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _simulator = simulator;
    }

    [AutomaticRetry(Attempts = MaxRetryAttempts, DelaysInSeconds = [30, 60, 120])]
    public async Task ExecuteAsync(
        Guid reserveHistoryId,
        Guid claimId,
        string idempotencyKey,
        PerformContext? context)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return;

        if (await _reserves.IsPostedForIdempotencyKeyAsync(idempotencyKey, CancellationToken.None))
            return;

        var history = await _reserves.GetHistoryByIdOnlyAsync(reserveHistoryId, CancellationToken.None);
        if (history is null)
            return;

        if (history.ClaimId != claimId || history.IdempotencyKey.Value != idempotencyKey)
            return;

        if (history.PostingStatus == ReservePostingStatus.Posted)
            return;

        var jobId = context?.BackgroundJob?.Id ?? "unknown";
        var journal =
            $"DR Change in Outstanding Reserves / CR Outstanding Loss Reserves, Amount = {history.Amount.Amount:C}.";

        var glAlreadyLogged = await _claims.HasAuditEntryAsync(
            history.ClaimId,
            AuditEventType.GlPostingSimulated,
            history.Id,
            CancellationToken.None);

        if (!glAlreadyLogged)
        {
            await _auditLog.WriteAsync(
                claimId: history.ClaimId,
                eventType: AuditEventType.GlPostingSimulated,
                description: journal,
                newValue: AuditJsonValues.JournalEntry(journal),
                relatedEntityId: history.Id,
                relatedEntityType: "ReserveHistory");
        }

        await _simulator.SimulateAsync(history, CancellationToken.None);

        if (history.PostingStatus != ReservePostingStatus.Posted)
        {
            history.MarkPosted(jobId);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
        }
    }
}
