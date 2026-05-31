using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimStatuses;

public sealed class GetClaimStatusesQueryHandler
    : IRequestHandler<GetClaimStatusesQuery, IReadOnlyList<ClaimStatusTransitionDto>>
{
    public Task<IReadOnlyList<ClaimStatusTransitionDto>> Handle(
        GetClaimStatusesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ClaimStatusTransitionDto> result = ClaimStatusTransitions
            .AllStatusTransitions()
            .Select(t => new ClaimStatusTransitionDto
            {
                Status = t.Status.ToString(),
                AllowedNextStatuses = t.AllowedNext
            })
            .ToList();

        return Task.FromResult(result);
    }
}
