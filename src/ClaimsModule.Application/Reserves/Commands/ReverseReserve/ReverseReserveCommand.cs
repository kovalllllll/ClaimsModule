using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Reserves.Commands.ReverseReserve;

public sealed record ReverseReserveCommand(
    Guid ClaimId,
    Guid OrganisationId,
    ReserveComponentType Component,
    decimal Amount,
    string ChangeReason,
    string? IdempotencyKey
) : ICommand<ReverseReserveResult>;

public sealed record ReverseReserveResult(
    Guid ReserveHistoryId,
    string ApprovalStatus,
    bool AutoApproved
);
