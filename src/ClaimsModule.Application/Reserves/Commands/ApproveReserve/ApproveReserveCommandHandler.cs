using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.Common.Validation;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.ApproveReserve;

public sealed class ApproveReserveCommandHandler(
    IClaimRepository claims,
    IReserveRepository reserves,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog,
    ICurrentUserService currentUser,
    ISystemClock clock)
    : IRequestHandler<ApproveReserveCommand, Unit>
{
    private const decimal SupervisorMaxSingle = 100_000m;
    private const decimal AggregateLimit = 10_000_000m;

    public async Task<Unit> Handle(ApproveReserveCommand request, CancellationToken cancellationToken)
    {
        var history = await reserves.GetHistoryByIdAsync(
                request.ReserveHistoryId, request.ClaimId, cancellationToken)
            ?? throw new KeyNotFoundException($"Reserve history {request.ReserveHistoryId} not found.");

        if (history.ApprovalStatus != ReserveApprovalStatus.PendingApproval)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    "ApprovalStatus",
                    $"Reserve is not pending approval. Current status: {history.ApprovalStatus}.")
            });

        if (history.SubmittedByUserId is { } submitterId
            && submitterId == currentUser.UserId)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    "Approver",
                    "Self-approval is not permitted.")
            });

        var amount = history.Amount.Amount;
        var role = currentUser.Role ?? string.Empty;
        var isSupervisor = role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase);
        var isManager = role.Equals("Manager", StringComparison.OrdinalIgnoreCase);

        if (!isSupervisor && !isManager)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    "Role",
                    ClaimValidationMessages.ReserveApprovalInsufficientAuthority)
            });

        if (isSupervisor && amount > SupervisorMaxSingle)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    "Amount",
                    $"Supervisors can only approve reserves up to {SupervisorMaxSingle:C}. " +
                    "A Manager must approve this amount.")
            });

        var allComponents = await reserves.GetComponentsByClaimIdAsync(request.ClaimId, cancellationToken);
        var existingTotal = allComponents.Sum(rc => rc.CurrentAmount.Amount);
        var projectedTotal = existingTotal + amount;

        if (projectedTotal > AggregateLimit)
        {
            if (!isManager)
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        "Amount",
                        $"Approving this reserve would bring the total to {projectedTotal:C}, " +
                        $"exceeding the {AggregateLimit:C} aggregate limit. Only a Manager can override.")
                });

            if (!request.ManagerOverride)
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        "ManagerOverride",
                        $"Approving this reserve would bring the total to {projectedTotal:C}, " +
                        $"exceeding the {AggregateLimit:C} threshold. " +
                        "Set ManagerOverride = true to confirm.")
                });

            var claim = await claims.GetByIdForUpdateAsync(request.ClaimId, cancellationToken);
            claim?.SetManagerOverrideFlag(true);
        }

        var component = await reserves.GetComponentByIdAsync(history.ReserveComponentId, cancellationToken)
            ?? throw new KeyNotFoundException("Reserve component not found.");

        var approvedAt = clock.UtcNow;
        history.Approve(currentUser.UserId!.Value, approvedAt);
        component.Approve(history.Id, history.Amount);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLog.WriteAsync(
            claimId: request.ClaimId,
            eventType: AuditEventType.ReserveApproved,
            description: $"Reserve of {amount:C} approved. New component balance: {component.CurrentAmount.Amount:C}.",
            relatedEntityId: history.Id,
            relatedEntityType: "ReserveHistory",
            ct: cancellationToken);

        return Unit.Value;
    }
}
