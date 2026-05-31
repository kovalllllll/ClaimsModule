using ClaimsModule.Application.Common.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.RetractReserve;

public sealed record RetractReserveCommand(
    Guid ReserveHistoryId,
    Guid ClaimId,
    Guid OrganisationId
) : ICommand<Unit>;
