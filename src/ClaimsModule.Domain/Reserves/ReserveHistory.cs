using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.ValueObjects;

namespace ClaimsModule.Domain.Reserves;

public sealed class ReserveHistory : Entity, ITenantScoped, IAuditable
{
    public Guid OrganisationId { get; private set; }
    public Guid ReserveComponentId { get; private set; }
    public Guid ClaimId { get; private set; }
    public ReserveTransactionType TransactionType { get; private set; }
    public Money Amount { get; private set; } = Money.Zero;
    public Money PreviousBalance { get; private set; } = Money.Zero;
    public Money NewBalance { get; private set; } = Money.Zero;
    public ReserveApprovalStatus ApprovalStatus { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? RejectedByUserId { get; private set; }
    public DateTimeOffset? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public string ChangeReason { get; private set; } = string.Empty;
    public ReservePostingStatus PostingStatus { get; private set; } = ReservePostingStatus.Pending;
    public string? PostingJobId { get; private set; }
    public IdempotencyKey IdempotencyKey { get; private set; } = null!;
    public int ChangeSequence { get; private set; }
    public Guid? SubmittedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UserCreated { get; set; }
    public Guid? UserModified { get; set; }

    private ReserveHistory() { }

    public static ReserveHistory Create(
        Guid reserveComponentId,
        Guid claimId,
        Guid organisationId,
        ReserveTransactionType transactionType,
        Money amount,
        Money previousBalance,
        Money newBalance,
        ReserveApprovalStatus approvalStatus,
        string changeReason,
        IdempotencyKey idempotencyKey,
        int changeSequence,
        Guid? submittedByUserId,
        DateTimeOffset createdAt)
        => new()
        {
            Id = EntityId.New(),
            ReserveComponentId = reserveComponentId,
            ClaimId = claimId,
            OrganisationId = organisationId,
            TransactionType = transactionType,
            Amount = amount,
            PreviousBalance = previousBalance,
            NewBalance = newBalance,
            ApprovalStatus = approvalStatus,
            ChangeReason = changeReason,
            IdempotencyKey = idempotencyKey,
            ChangeSequence = changeSequence,
            SubmittedByUserId = submittedByUserId,
            CreatedAt = createdAt,
            PostingStatus = ReservePostingStatus.Pending
        };

    public void Approve(Guid approvedByUserId, DateTimeOffset approvedAt)
    {
        ApprovalStatus = ReserveApprovalStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = approvedAt;
        NewBalance = PreviousBalance.Add(Amount);
    }

    public void AutoApprove(Guid approvedByUserId, DateTimeOffset approvedAt)
    {
        ApprovalStatus = ReserveApprovalStatus.AutoApproved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = approvedAt;
    }

    public void Reject(Guid rejectedByUserId, DateTimeOffset rejectedAt, string rejectionReason)
    {
        ApprovalStatus = ReserveApprovalStatus.Rejected;
        RejectedByUserId = rejectedByUserId;
        RejectedAt = rejectedAt;
        RejectionReason = rejectionReason;
    }

    public void Retract()
    {
        ApprovalStatus = ReserveApprovalStatus.Cancelled;
    }

    public void MarkPosted(string jobId)
    {
        PostingStatus = ReservePostingStatus.Posted;
        PostingJobId = jobId;
    }

    public void MarkPostingFailed()
    {
        PostingStatus = ReservePostingStatus.Failed;
    }

    public void ResetPostingForRetry()
    {
        PostingStatus = ReservePostingStatus.Pending;
        PostingJobId = null;
    }
}
