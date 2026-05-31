using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.RetractReserve;

public sealed class RetractReserveCommandHandler(
    IReserveRepository reserves,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog,
    ICurrentUserService currentUser)
    : IRequestHandler<RetractReserveCommand, Unit>
{
    public async Task<Unit> Handle(RetractReserveCommand request, CancellationToken cancellationToken)
    {
        var history = await reserves.GetHistoryByIdAsync(
                request.ReserveHistoryId, request.ClaimId, cancellationToken)
            ?? throw new KeyNotFoundException($"Reserve history {request.ReserveHistoryId} not found.");

        if (history.ApprovalStatus != ReserveApprovalStatus.PendingApproval)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("ApprovalStatus",
                    $"Only PendingApproval reserves can be retracted. Current status: {history.ApprovalStatus}.")
            });

        if (history.SubmittedByUserId != currentUser.UserId)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("UserId",
                    "Only the submitter can retract a reserve request.")
            });

        history.Retract();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLog.WriteAsync(
            claimId: request.ClaimId,
            eventType: AuditEventType.ReserveRetracted,
            description: "Reserve retracted by submitter.",
            relatedEntityId: history.Id,
            relatedEntityType: "ReserveHistory",
            ct: cancellationToken);

        return Unit.Value;
    }
}
