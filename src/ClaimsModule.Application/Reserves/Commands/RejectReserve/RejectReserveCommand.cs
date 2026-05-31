using ClaimsModule.Application.Common.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.RejectReserve;

public sealed record RejectReserveCommand(
    Guid ReserveHistoryId,
    Guid ClaimId,
    Guid OrganisationId,
    string RejectionReason
) : ICommand<Unit>;
