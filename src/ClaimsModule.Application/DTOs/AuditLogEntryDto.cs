namespace ClaimsModule.Application.DTOs;

public sealed class AuditLogEntryDto
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? CorrelationId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid? CreatedByUserId { get; init; }
}
