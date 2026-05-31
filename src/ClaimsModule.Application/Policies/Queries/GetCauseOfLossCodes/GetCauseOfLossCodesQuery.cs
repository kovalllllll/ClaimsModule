using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Policies.Queries.GetCauseOfLossCodes;

public sealed record GetCauseOfLossCodesQuery(
    Guid OrganisationId,
    string? PerilCategory = null
) : IRequest<IReadOnlyList<CauseOfLossCodeDto>>;
