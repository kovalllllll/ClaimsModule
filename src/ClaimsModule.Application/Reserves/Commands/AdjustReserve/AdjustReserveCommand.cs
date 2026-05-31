using ClaimsModule.Application.Common.Interfaces;

namespace ClaimsModule.Application.Reserves.Commands.AdjustReserve;

/// <summary>
/// Creates a new ReserveHistory row with TransactionType = Adjust on an existing component.
/// Amount is a signed delta: positive increases the balance, negative decreases it.
/// SubrogationRecoverable may carry a negative delta by design (BR-R-01).
/// </summary>
public sealed record AdjustReserveCommand(
    Guid ReserveComponentId,
    Guid ClaimId,
    Guid OrganisationId,
    decimal Amount,
    string ChangeReason,
    string? IdempotencyKey
) : ICommand<AdjustReserveResult>;

public sealed record AdjustReserveResult(
    Guid ReserveHistoryId,
    string ApprovalStatus,
    bool AutoApproved
);
