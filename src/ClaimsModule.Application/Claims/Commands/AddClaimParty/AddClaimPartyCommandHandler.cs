using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Parties;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.AddClaimParty;

public sealed class AddClaimPartyCommandHandler(
    IClaimRepository claims,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog)
    : IRequestHandler<AddClaimPartyCommand, Guid>
{
    public async Task<Guid> Handle(AddClaimPartyCommand request, CancellationToken cancellationToken)
    {
        if (!await claims.ExistsAsync(request.ClaimId, request.OrganisationId, cancellationToken))
            throw new KeyNotFoundException($"Claim {request.ClaimId} not found.");

        var party = ClaimParty.Create(
            claimId: request.ClaimId,
            organisationId: request.OrganisationId,
            partyRole: request.PartyRole,
            partyType: request.PartyType,
            firstName: request.FirstName,
            lastName: request.LastName,
            companyName: request.CompanyName,
            email: request.Email,
            phone: request.Phone,
            notes: request.Notes);

        await claims.AddPartyAsync(party, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLog.WriteAsync(
            claimId: request.ClaimId,
            eventType: AuditEventType.PartyAdded,
            description: $"Party {request.PartyRole} added.",
            relatedEntityId: party.Id,
            relatedEntityType: nameof(ClaimParty),
            ct: cancellationToken);

        return party.Id;
    }
}
