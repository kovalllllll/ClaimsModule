using AutoMapper;
using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Policies.Queries.GetCauseOfLossCodes;

public sealed class GetCauseOfLossCodesQueryHandler(IPolicyRepository policies, IMapper mapper)
    : IRequestHandler<GetCauseOfLossCodesQuery, IReadOnlyList<CauseOfLossCodeDto>>
{
    public async Task<IReadOnlyList<CauseOfLossCodeDto>> Handle(
        GetCauseOfLossCodesQuery request,
        CancellationToken cancellationToken)
    {
        var codes = await policies.GetCauseOfLossCodesAsync(
            request.OrganisationId, request.PerilCategory, cancellationToken);

        return codes.Select(mapper.Map<CauseOfLossCodeDto>).ToList();
    }
}
