using ClaimsModule.Application.Common.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.RemoveClaimParty;

public sealed record RemoveClaimPartyCommand(
    Guid ClaimId,
    Guid PartyId,
    Guid OrganisationId
) : ICommand<Unit>;
