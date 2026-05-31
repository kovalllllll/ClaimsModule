using ClaimsModule.Application.Common.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.UpdateClaimNotes;

public sealed record UpdateClaimNotesCommand(
    Guid ClaimId,
    Guid OrganisationId,
    string? Notes
) : ICommand<Unit>;
