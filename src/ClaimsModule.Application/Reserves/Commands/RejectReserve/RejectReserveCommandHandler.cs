using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.RejectReserve;

public sealed class RejectReserveCommandHandler(
    IReserveRepository reserves,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ISystemClock clock)
    : IRequestHandler<RejectReserveCommand, Unit>
{
    public async Task<Unit> Handle(RejectReserveCommand request, CancellationToken cancellationToken)
    {
        var history = await reserves.GetHistoryByIdAsync(
                request.ReserveHistoryId, request.ClaimId, cancellationToken)
            ?? throw new KeyNotFoundException($"Reserve history {request.ReserveHistoryId} not found.");

        if (history.ApprovalStatus != ReserveApprovalStatus.PendingApproval)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("ApprovalStatus",
                    $"Reserve history is not pending approval. Current status: {history.ApprovalStatus}.")
            });

        var role = currentUser.Role ?? string.Empty;
        if (!role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase)
            && !role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Role",
                    "Only Supervisors or Managers can reject reserve requests.")
            });

        var component = await reserves.GetComponentByIdAsync(history.ReserveComponentId, cancellationToken)
            ?? throw new KeyNotFoundException("Reserve component not found.");

        history.Reject(currentUser.UserId!.Value, clock.UtcNow, request.RejectionReason);
        component.Reject(history.Id, request.RejectionReason);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
