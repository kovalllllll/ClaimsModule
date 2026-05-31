using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimStatuses;

public sealed record GetClaimStatusesQuery : IRequest<IReadOnlyList<ClaimStatusTransitionDto>>;
