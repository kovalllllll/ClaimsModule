using AutoMapper;
using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;
using MediatR;

namespace ClaimsModule.Application.Reserves.Queries.GetClaimReserves;

public sealed class GetClaimReservesQueryHandler(
    IClaimRepository claims,
    IReserveRepository reserves,
    IMapper mapper)
    : IRequestHandler<GetClaimReservesQuery, ReserveDetailDto?>
{
    public async Task<ReserveDetailDto?> Handle(
        GetClaimReservesQuery request,
        CancellationToken cancellationToken)
    {
        if (!await claims.ExistsAsync(request.ClaimId, request.OrganisationId, cancellationToken))
            return null;

        var components = (await reserves.GetComponentsWithHistoryAsync(request.ClaimId, cancellationToken))
            .OrderBy(rc => rc.Component)
            .ToList();

        var componentDtos = components.Select(MapComponent).ToList();
        var transactionDtos = components
            .SelectMany(rc => rc.History.Select(h => MapTransaction(h, rc)))
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        return new ReserveDetailDto
        {
            ClaimId = request.ClaimId,
            Components = componentDtos,
            Transactions = transactionDtos,
            TotalApprovedAmount = components.Sum(rc => rc.CurrentAmount.Amount)
        };
    }

    private ReserveComponentSummaryDto MapComponent(ClaimReserveComponent rc)
    {
        var dto = mapper.Map<ReserveComponentSummaryDto>(rc);
        var pending = rc.History
            .Where(h => h.ApprovalStatus == ReserveApprovalStatus.PendingApproval)
            .ToList();
        return new ReserveComponentSummaryDto
        {
            Id = dto.Id,
            ComponentType = dto.ComponentType,
            CurrentAmount = dto.CurrentAmount,
            Status = dto.Status,
            Notes = dto.Notes,
            HasPendingApproval = pending.Count > 0,
            PendingAmount = pending.Count > 0 ? pending.Sum(h => h.Amount.Amount) : null
        };
    }

    private ReserveTransactionDto MapTransaction(ReserveHistory history, ClaimReserveComponent component)
    {
        var dto = mapper.Map<ReserveTransactionDto>(history);
        return new ReserveTransactionDto
        {
            Id = dto.Id,
            ReserveComponentId = dto.ReserveComponentId,
            ComponentType = component.Component.ToString(),
            TransactionType = dto.TransactionType,
            Amount = dto.Amount,
            PreviousBalance = dto.PreviousBalance,
            NewBalance = dto.NewBalance,
            ApprovalStatus = dto.ApprovalStatus,
            ApprovedByUserId = dto.ApprovedByUserId,
            ApprovedAt = dto.ApprovedAt,
            RejectedByUserId = dto.RejectedByUserId,
            RejectedAt = dto.RejectedAt,
            RejectionReason = dto.RejectionReason,
            ChangeReason = dto.ChangeReason,
            PostingStatus = dto.PostingStatus,
            IdempotencyKey = dto.IdempotencyKey,
            ChangeSequence = dto.ChangeSequence,
            SubmittedByUserId = dto.SubmittedByUserId,
            CreatedAt = dto.CreatedAt
        };
    }
}
