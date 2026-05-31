using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common.Validation;
using ClaimsModule.Application.Reserves;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.ValueObjects;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.ReverseReserve;

public sealed class ReverseReserveCommandHandler(
    IReserveRepository reserves,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog,
    ICurrentUserService currentUser,
    ISystemClock clock,
    ReserveApiIdempotency apiIdempotency,
    IValidationWarningCollector warnings)
    : IRequestHandler<ReverseReserveCommand, ReverseReserveResult>
{
    private const decimal AutoApprovalThreshold = 10_000m;

    public async Task<ReverseReserveResult> Handle(
        ReverseReserveCommand request,
        CancellationToken cancellationToken)
    {
        var cachedId = await apiIdempotency.TryGetCachedHistoryIdAsync(
            request.OrganisationId,
            ReserveApiIdempotencyOperations.ReverseReserve,
            request.IdempotencyKey,
            cancellationToken);

        if (cachedId is not null)
        {
            var cached = await apiIdempotency.GetHistoryOrThrowAsync(cachedId.Value, cancellationToken);
            return ReserveApiIdempotency.ToReverseResult(cached);
        }

        var component = await reserves.GetComponentByTypeAsync(
                request.ClaimId, request.OrganisationId, request.Component, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Reserve component {request.Component} not found for claim {request.ClaimId}.");

        var nextSequence = await reserves.GetNextChangeSequenceAsync(component.Id, cancellationToken);
        var reverseAmount = new Money(request.Amount);
        var previousBalance = component.CurrentAmount;
        var now = clock.UtcNow;
        bool autoApprove = Math.Abs(request.Amount) <= AutoApprovalThreshold;
        var approvalStatus = autoApprove
            ? ReserveApprovalStatus.AutoApproved
            : ReserveApprovalStatus.PendingApproval;

        var history = ReserveHistory.Create(
            reserveComponentId: component.Id,
            claimId: request.ClaimId,
            organisationId: request.OrganisationId,
            transactionType: ReserveTransactionType.Reverse,
            amount: reverseAmount,
            previousBalance: previousBalance,
            newBalance: autoApprove ? previousBalance.Add(reverseAmount) : previousBalance,
            approvalStatus: approvalStatus,
            changeReason: request.ChangeReason,
            idempotencyKey: IdempotencyKey.ForReserveChange(component.Id, nextSequence),
            changeSequence: nextSequence,
            submittedByUserId: currentUser.UserId,
            createdAt: now);

        if (autoApprove)
        {
            history.AutoApprove(currentUser.UserId!.Value, now);
            component.Approve(history.Id, reverseAmount);
        }

        await reserves.AddHistoryAsync(history, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var auditEventType = autoApprove
            ? AuditEventType.ReserveAutoApproved
            : AuditEventType.ReserveCreated;

        await auditLog.WriteAsync(
            claimId: request.ClaimId,
            eventType: auditEventType,
            description: $"Reserve {component.Component} reversed by {request.Amount:C}. " +
                         $"Previous balance: {previousBalance.Amount:C}.",
            relatedEntityId: history.Id,
            relatedEntityType: nameof(ReserveHistory),
            ct: cancellationToken);

        await warnings.WriteValidationWarningsToAuditAsync(
            auditLog, request.ClaimId, cancellationToken);

        await apiIdempotency.RecordAsync(
            request.OrganisationId,
            ReserveApiIdempotencyOperations.ReverseReserve,
            request.IdempotencyKey,
            history.Id,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReverseReserveResult(history.Id, approvalStatus.ToString(), autoApprove);
    }
}
