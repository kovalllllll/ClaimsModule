namespace ClaimsModule.Application.DTOs;

public sealed class ReserveTransactionDto
{
    public Guid Id { get; init; }
    public Guid ReserveComponentId { get; init; }
    public string ComponentType { get; init; } = string.Empty;
    public string TransactionType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal PreviousBalance { get; init; }
    public decimal NewBalance { get; init; }
    public string ApprovalStatus { get; init; } = string.Empty;
    public Guid? ApprovedByUserId { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public Guid? RejectedByUserId { get; init; }
    public DateTimeOffset? RejectedAt { get; init; }
    public string? RejectionReason { get; init; }
    public string ChangeReason { get; init; } = string.Empty;
    public string PostingStatus { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
    public int ChangeSequence { get; init; }
    public Guid? SubmittedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
