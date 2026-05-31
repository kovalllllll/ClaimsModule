using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Reserves.Commands.OpenReserve;

public sealed record OpenReserveCommand(
    Guid ClaimId,
    Guid OrganisationId,
    ReserveComponentType ComponentType,
    decimal Amount,
    string ChangeReason,
    string? IdempotencyKey
) : ICommand<OpenReserveResult>;

public sealed record OpenReserveResult(
    Guid ReserveHistoryId,
    string ApprovalStatus,
    bool AutoApproved
);
