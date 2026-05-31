using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.RemoveClaimParty;

public sealed class RemoveClaimPartyCommandHandler(
    IClaimRepository claims,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog)
    : IRequestHandler<RemoveClaimPartyCommand, Unit>
{
    public async Task<Unit> Handle(RemoveClaimPartyCommand request, CancellationToken cancellationToken)
    {
        var party = await claims.GetPartyAsync(
                request.PartyId, request.ClaimId, request.OrganisationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Party {request.PartyId} not found.");

        if (!party.IsActive)
            throw new KeyNotFoundException($"Party {request.PartyId} not found.");

        if (party.PartyRole == PartyRole.Claimant)
        {
            var activeClaimants = await claims.CountActiveClaimantsAsync(request.ClaimId, cancellationToken);
            if (activeClaimants <= 1)
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("PartyId",
                        "Cannot remove the last Claimant from a claim.")
                });
        }

        party.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLog.WriteAsync(
            claimId: request.ClaimId,
            eventType: AuditEventType.PartyRemoved,
            description: $"Party {party.PartyRole} removed.",
            relatedEntityId: party.Id,
            relatedEntityType: "ClaimParty",
            ct: cancellationToken);

        return Unit.Value;
    }
}
