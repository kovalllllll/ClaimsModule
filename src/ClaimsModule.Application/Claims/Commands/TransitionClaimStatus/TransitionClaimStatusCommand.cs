using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.TransitionClaimStatus;

public sealed record TransitionClaimStatusCommand(
    Guid ClaimId,
    Guid OrganisationId,
    ClaimStatus TargetStatus,
    string? Reason = null,
    string? RowVer = null
) : ICommand<Unit>;
