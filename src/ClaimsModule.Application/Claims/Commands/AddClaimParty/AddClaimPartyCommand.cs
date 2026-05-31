using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Claims.Commands.AddClaimParty;

public sealed record AddClaimPartyCommand(
    Guid ClaimId,
    Guid OrganisationId,
    PartyRole PartyRole,
    PartyType PartyType,
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? Email,
    string? Phone,
    string? Notes = null
) : ICommand<Guid>;
