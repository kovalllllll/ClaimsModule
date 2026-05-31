using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Abstractions.Services;

public interface IAuditLogService
{
    Task WriteAsync(
        Guid claimId,
        AuditEventType eventType,
        string description,
        Guid? correlationId = null,
        string? oldValue = null,
        string? newValue = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken ct = default);
}
