using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.ListClaims;

public sealed record ListClaimsQuery(
    Guid OrganisationId,
    ClaimStatus? Status = null,
    IReadOnlyList<ClaimStatus>? Statuses = null,
    DateTimeOffset? DateFrom = null,
    DateTimeOffset? DateTo = null,
    Guid? AssignedHandlerId = null,
    string? AssignedHandlerSearch = null,
    string? CauseOfLossCode = null,
    Guid? PolicyId = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<ClaimSummaryDto>>;
