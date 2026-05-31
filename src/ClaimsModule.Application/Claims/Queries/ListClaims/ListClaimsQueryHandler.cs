using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.ListClaims;

public sealed class ListClaimsQueryHandler(
    IClaimRepository claims,
    IReserveRepository reserves,
    IPolicyRepository policies)
    : IRequestHandler<ListClaimsQuery, PagedResult<ClaimSummaryDto>>
{
    public async Task<PagedResult<ClaimSummaryDto>> Handle(
        ListClaimsQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = request.Statuses is { Count: > 0 }
            ? request.Statuses
            : request.Status.HasValue
                ? new[] { request.Status.Value }
                : null;

        var page = await claims.ListAsync(new ClaimListCriteria(
            request.OrganisationId,
            request.Status,
            statuses,
            request.DateFrom,
            request.DateTo,
            request.AssignedHandlerId,
            request.AssignedHandlerSearch,
            request.CauseOfLossCode,
            request.PolicyId,
            request.Search,
            request.PageNumber,
            request.PageSize), cancellationToken);

        if (page.TotalCount == 0)
            return PagedResult<ClaimSummaryDto>.Create([], 0, request.PageNumber, request.PageSize);

        var claimIds = page.Claims.Select(c => c.Id).ToList();
        var reserveComponents = await reserves.GetComponentsForClaimIdsAsync(claimIds, cancellationToken);
        var reserveTotals = reserveComponents
            .GroupBy(rc => rc.ClaimId)
            .ToDictionary(g => g.Key, g => g.Sum(rc => rc.CurrentAmount.Amount));

        var causeCodes = await policies.GetCauseOfLossCodeNamesAsync(cancellationToken);

        var items = page.Claims.Select(c =>
        {
            var primaryLoss = c.LossEvents.OrderBy(le => le.LossDate).FirstOrDefault();
            var colCode = primaryLoss?.CauseOfLossCode;
            return new ClaimSummaryDto
            {
                Id = c.Id,
                ClaimNumber = c.ClaimNumber.Value,
                ClientName = c.ClientName,
                PolicyNumber = c.PolicyNumber,
                LossDate = primaryLoss?.LossDate,
                CauseOfLossCode = colCode,
                CauseOfLossName = colCode is not null && causeCodes.TryGetValue(colCode, out var name)
                    ? name
                    : null,
                Status = c.Status.ToString(),
                Severity = c.Severity?.ToString(),
                TotalReserves = reserveTotals.GetValueOrDefault(c.Id, 0m),
                ReportedDate = c.ReportedDate,
                CreatedAt = c.CreatedAt,
                AssignedHandlerId = c.AssignedHandlerId,
                AssignedHandlerName = MockUserNames.Resolve(c.AssignedHandlerId)
            };
        }).ToList();

        return PagedResult<ClaimSummaryDto>.Create(items, page.TotalCount, request.PageNumber, request.PageSize);
    }
}
