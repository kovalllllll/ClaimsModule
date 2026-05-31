using ClaimsModule.Application.Common.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.ApproveReserve;

public sealed record ApproveReserveCommand(
    Guid ReserveHistoryId,
    Guid ClaimId,
    Guid OrganisationId,
    bool ManagerOverride = false
) : ICommand<Unit>;
