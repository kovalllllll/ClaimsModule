using ClaimsModule.Application.Common.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.RetryGlPosting;

public sealed record RetryGlPostingCommand(
    Guid ReserveHistoryId,
    Guid ClaimId,
    Guid OrganisationId
) : ICommand<Unit>;
