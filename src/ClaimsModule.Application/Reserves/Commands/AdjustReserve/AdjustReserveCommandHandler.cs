using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common.Validation;
using ClaimsModule.Application.Reserves;
using ClaimsModule.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.ValueObjects;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.AdjustReserve;

public sealed class AdjustReserveCommandHandler(
    IReserveRepository reserves,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog,
    ICurrentUserService currentUser,
    ISystemClock clock,
    ReserveApiIdempotency apiIdempotency,
    IValidationWarningCollector warnings)
    : IRequestHandler<AdjustReserveCommand, AdjustReserveResult>
{
    private const decimal AutoApprovalThreshold = 10_000m;

    public async Task<AdjustReserveResult> Handle(
        AdjustReserveCommand request,
        CancellationToken cancellationToken)
    {
        var cachedId = await apiIdempotency.TryGetCachedHistoryIdAsync(
            request.OrganisationId,
            ReserveApiIdempotencyOperations.AdjustReserve,
            request.IdempotencyKey,
            cancellationToken);

        if (cachedId is not null)
        {
            var cached = await apiIdempotency.GetHistoryOrThrowAsync(cachedId.Value, cancellationToken);
            return ReserveApiIdempotency.ToAdjustResult(cached);
        }

        var component = await reserves.GetComponentByIdAsync(request.ReserveComponentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Reserve component {request.ReserveComponentId} not found.");

        if (component.ClaimId != request.ClaimId)
            throw new KeyNotFoundException($"Reserve component {request.ReserveComponentId} not found.");

        var nextSequence = await reserves.GetNextChangeSequenceAsync(component.Id, cancellationToken);
        var adjustmentAmount = new Money(request.Amount);
        var previousBalance = component.CurrentAmount;
        var now = clock.UtcNow;
        bool autoApprove = Math.Abs(request.Amount) <= AutoApprovalThreshold;
        var approvalStatus = autoApprove
            ? ReserveApprovalStatus.AutoApproved
            : ReserveApprovalStatus.PendingApproval;

        var projectedBalanceAmount = autoApprove
            ? previousBalance.Add(adjustmentAmount).Amount
            : previousBalance.Amount;

        if (ReserveAmountRules.ViolatesNonNegativeBalanceRule(component.Component, projectedBalanceAmount))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Amount), ReserveAmountRules.NonNegativeBalanceMessage)
            });
        }

        var history = ReserveHistory.Create(
            reserveComponentId: component.Id,
            claimId: request.ClaimId,
            organisationId: request.OrganisationId,
            transactionType: ReserveTransactionType.Adjust,
            amount: adjustmentAmount,
            previousBalance: previousBalance,
            newBalance: autoApprove ? previousBalance.Add(adjustmentAmount) : previousBalance,
            approvalStatus: approvalStatus,
            changeReason: request.ChangeReason,
            idempotencyKey: IdempotencyKey.ForReserveChange(component.Id, nextSequence),
            changeSequence: nextSequence,
            submittedByUserId: currentUser.UserId,
            createdAt: now);

        if (autoApprove)
        {
            history.AutoApprove(currentUser.UserId!.Value, now);
            component.Approve(history.Id, adjustmentAmount);
        }

        await reserves.AddHistoryAsync(history, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var auditEventType = autoApprove
            ? AuditEventType.ReserveAutoApproved
            : AuditEventType.ReserveCreated;

        await auditLog.WriteAsync(
            claimId: request.ClaimId,
            eventType: auditEventType,
            description: $"Reserve {component.Component} adjusted by {request.Amount:C}. " +
                         $"Previous balance: {previousBalance.Amount:C}.",
            relatedEntityId: history.Id,
            relatedEntityType: nameof(ReserveHistory),
            ct: cancellationToken);

        await warnings.WriteValidationWarningsToAuditAsync(
            auditLog, request.ClaimId, cancellationToken);

        await apiIdempotency.RecordAsync(
            request.OrganisationId,
            ReserveApiIdempotencyOperations.AdjustReserve,
            request.IdempotencyKey,
            history.Id,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AdjustReserveResult(history.Id, approvalStatus.ToString(), autoApprove);
    }
}
