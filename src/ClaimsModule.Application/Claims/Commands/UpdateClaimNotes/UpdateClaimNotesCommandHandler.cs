using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.UpdateClaimNotes;

public sealed class UpdateClaimNotesCommandHandler(
    IClaimRepository claims,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog)
    : IRequestHandler<UpdateClaimNotesCommand, Unit>
{
    public async Task<Unit> Handle(UpdateClaimNotesCommand request, CancellationToken cancellationToken)
    {
        var claim = await claims.GetByIdWithPartiesForUpdateAsync(
                request.ClaimId, request.OrganisationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim {request.ClaimId} not found.");

        claim.UpdateNotes(request.Notes);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLog.WriteAsync(
            claimId: claim.Id,
            eventType: AuditEventType.ValidationIssueAdded,
            description: "[Info] Claim notes updated.",
            newValue: AuditJsonValues.Notes(request.Notes),
            ct: cancellationToken);

        return Unit.Value;
    }
}
