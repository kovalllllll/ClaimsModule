using ClaimsModule.Application.Common.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.LinkPolicy;

public sealed record LinkPolicyCommand(
    Guid ClaimId,
    Guid OrganisationId,
    Guid PolicyId
) : ICommand<Unit>;
