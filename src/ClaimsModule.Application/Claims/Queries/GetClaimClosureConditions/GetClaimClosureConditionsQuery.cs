using ClaimsModule.Application.Abstractions.Services;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimClosureConditions;

public sealed record GetClaimClosureConditionsQuery(
    Guid ClaimId,
    Guid OrganisationId,
    string? ClosureReason = null
) : IRequest<ClaimClosureConditionsResult?>;
