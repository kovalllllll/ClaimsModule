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

namespace ClaimsModule.Application.Reserves.Commands.OpenReserve;

public sealed class OpenReserveCommandHandler(
    IReserveRepository reserves,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog,
    ICurrentUserService currentUser,
    ISystemClock clock,
    ReserveApiIdempotency apiIdempotency,
    IValidationWarningCollector warnings)
    : IRequestHandler<OpenReserveCommand, OpenReserveResult>
{
    private const decimal AutoApprovalThreshold = 10_000m;

    public async Task<OpenReserveResult> Handle(OpenReserveCommand request, CancellationToken cancellationToken)
    {
        var cachedId = await apiIdempotency.TryGetCachedHistoryIdAsync(
            request.OrganisationId,
            ReserveApiIdempotencyOperations.OpenReserve,
            request.IdempotencyKey,
            cancellationToken);

        if (cachedId is not null)
        {
            var cached = await apiIdempotency.GetHistoryOrThrowAsync(cachedId.Value, cancellationToken);
            return ReserveApiIdempotency.ToOpenResult(cached);
        }

        var component = await reserves.GetComponentByTypeAsync(
            request.ClaimId, request.OrganisationId, request.ComponentType, cancellationToken);

        if (component is null)
        {
            component = ClaimReserveComponent.Create(
                claimId: request.ClaimId,
                organisationId: request.OrganisationId,
                component: request.ComponentType);
            await reserves.AddComponentAsync(component, cancellationToken);
        }

        var nextSequence = await reserves.GetNextChangeSequenceAsync(component.Id, cancellationToken);
        var newAmount = new Money(request.Amount);
        var previousBalance = component.CurrentAmount;
        var now = clock.UtcNow;
        bool autoApprove = Math.Abs(request.Amount) <= AutoApprovalThreshold;
        var approvalStatus = autoApprove
            ? ReserveApprovalStatus.AutoApproved
            : ReserveApprovalStatus.PendingApproval;

        var projectedBalanceAmount = autoApprove
            ? previousBalance.Add(newAmount).Amount
            : previousBalance.Amount;

        if (ReserveAmountRules.ViolatesNonNegativeBalanceRule(
                request.ComponentType,
                projectedBalanceAmount))
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
            transactionType: ReserveTransactionType.Add,
            amount: newAmount,
            previousBalance: previousBalance,
            newBalance: autoApprove ? previousBalance.Add(newAmount) : previousBalance,
            approvalStatus: approvalStatus,
            changeReason: request.ChangeReason,
            idempotencyKey: IdempotencyKey.ForReserveChange(component.Id, nextSequence),
            changeSequence: nextSequence,
            submittedByUserId: currentUser.UserId,
            createdAt: now);

        if (autoApprove)
        {
            history.AutoApprove(currentUser.UserId!.Value, now);
            component.Approve(history.Id, newAmount);
        }

        await reserves.AddHistoryAsync(history, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var auditEventType = autoApprove ? AuditEventType.ReserveAutoApproved : AuditEventType.ReserveCreated;
        await auditLog.WriteAsync(
            claimId: request.ClaimId,
            eventType: auditEventType,
            description: $"Reserve {request.ComponentType} opened for {request.Amount:C}.",
            relatedEntityId: history.Id,
            relatedEntityType: nameof(ReserveHistory),
            ct: cancellationToken);

        await warnings.WriteValidationWarningsToAuditAsync(
            auditLog, request.ClaimId, cancellationToken);

        await apiIdempotency.RecordAsync(
            request.OrganisationId,
            ReserveApiIdempotencyOperations.OpenReserve,
            request.IdempotencyKey,
            history.Id,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OpenReserveResult(history.Id, approvalStatus.ToString(), autoApprove);
    }
}
