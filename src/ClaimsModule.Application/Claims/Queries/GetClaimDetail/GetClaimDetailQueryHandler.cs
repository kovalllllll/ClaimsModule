using AutoMapper;
using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimDetail;

public sealed class GetClaimDetailQueryHandler(
    IClaimRepository claims,
    IReserveRepository reserves,
    IMapper mapper,
    IStorageService storage)
    : IRequestHandler<GetClaimDetailQuery, ClaimDetailDto?>
{
    public async Task<ClaimDetailDto?> Handle(GetClaimDetailQuery request, CancellationToken cancellationToken)
    {
        var claim = await claims.GetDetailByIdAsync(
            request.ClaimId, request.OrganisationId, cancellationToken);

        if (claim is null) return null;

        var reserveComponents = await reserves.GetComponentsWithHistoryAsync(
            request.ClaimId, cancellationToken);

        var recentAudit = await claims.GetRecentAuditAsync(request.ClaimId, 20, cancellationToken);

        var documents = new List<ClaimDocumentDto>();
        foreach (var doc in claim.Documents)
        {
            var blobPath = StorageBlobPathNormalizer.ResolveReadPath(doc.BlobPath);
            var sasUrl = await storage.GetSasUrlAsync(blobPath, TimeSpan.FromHours(1), cancellationToken);
            documents.Add(new ClaimDocumentDto
            {
                Id = doc.Id,
                DocumentType = doc.DocumentType.ToString(),
                DocumentName = doc.DocumentName,
                ContentType = doc.ContentType,
                FileSizeBytes = doc.FileSizeBytes,
                UploadedAt = doc.UploadedAt,
                UploadedByUserId = doc.UploadedByUserId,
                Notes = doc.Notes,
                SasUrl = sasUrl
            });
        }

        var componentDtos = reserveComponents.Select(rc =>
        {
            var pending = rc.History
                .Where(h => h.ApprovalStatus == ReserveApprovalStatus.PendingApproval)
                .ToList();
            return new ReserveComponentSummaryDto
            {
                Id = rc.Id,
                ComponentType = rc.Component.ToString(),
                CurrentAmount = rc.CurrentAmount.Amount,
                Status = rc.Status.ToString(),
                HasPendingApproval = pending.Count > 0,
                PendingAmount = pending.Count > 0 ? pending.Sum(h => h.Amount.Amount) : null,
                Notes = rc.Notes
            };
        }).ToList();

        var transactionDtos = reserveComponents
            .SelectMany(rc => rc.History.Select(h => (Component: rc, History: h)))
            .OrderByDescending(x => x.History.CreatedAt)
            .Select(x =>
            {
                var mapped = mapper.Map<ReserveTransactionDto>(x.History);
                return new ReserveTransactionDto
                {
                    Id = mapped.Id,
                    ReserveComponentId = mapped.ReserveComponentId,
                    ComponentType = x.Component.Component.ToString(),
                    TransactionType = mapped.TransactionType,
                    Amount = mapped.Amount,
                    PreviousBalance = mapped.PreviousBalance,
                    NewBalance = mapped.NewBalance,
                    ApprovalStatus = mapped.ApprovalStatus,
                    ApprovedByUserId = mapped.ApprovedByUserId,
                    ApprovedAt = mapped.ApprovedAt,
                    RejectedByUserId = mapped.RejectedByUserId,
                    RejectedAt = mapped.RejectedAt,
                    RejectionReason = mapped.RejectionReason,
                    ChangeReason = mapped.ChangeReason,
                    PostingStatus = mapped.PostingStatus,
                    IdempotencyKey = mapped.IdempotencyKey,
                    ChangeSequence = mapped.ChangeSequence,
                    SubmittedByUserId = mapped.SubmittedByUserId,
                    CreatedAt = mapped.CreatedAt
                };
            })
            .ToList();

        var totalReserves = reserveComponents.Sum(rc => rc.CurrentAmount.Amount);
        var parties = await claims.GetPartiesForClaimAsync(
            request.ClaimId, request.OrganisationId, cancellationToken);
        var partyDtos = parties.Select(mapper.Map<ClaimPartyDto>).ToList();
        var riskObjectDtos = mapper.Map<List<ClaimRiskObjectDto>>(claim.RiskObjects);
        var lossEventDtos = mapper.Map<List<LossEventDto>>(claim.LossEvents);
        var auditDtos = recentAudit.Select(mapper.Map<AuditLogEntryDto>).ToList();
        var validNextStatuses = ClaimStatusTransitions.NextStatusNames(claim.Status);

        return new ClaimDetailDto
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber.Value,
            PolicyId = claim.PolicyId,
            PolicyNumber = claim.PolicyNumber,
            ClientName = claim.ClientName,
            Status = claim.Status.ToString(),
            ValidNextStatuses = validNextStatuses,
            Severity = claim.Severity?.ToString(),
            ReportedDate = claim.ReportedDate,
            AssignedHandlerId = claim.AssignedHandlerId,
            AssignedHandlerName = MockUserNames.Resolve(claim.AssignedHandlerId),
            RowVer = Convert.ToBase64String(claim.RowVer),
            ClosedAt = claim.ClosedAt,
            ClosureReason = claim.ClosureReason,
            Notes = claim.Notes,
            ManagerOverrideFlag = claim.ManagerOverrideFlag,
            CreatedAt = claim.CreatedAt,
            UpdatedAt = claim.UpdatedAt,
            LossEvents = lossEventDtos,
            Parties = partyDtos,
            RiskObjects = riskObjectDtos,
            ReserveComponents = componentDtos,
            ReserveTransactions = transactionDtos,
            TotalReserves = totalReserves,
            Documents = documents,
            RecentAuditEntries = auditDtos
        };
    }
}
