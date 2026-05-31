using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Common.Validation;

public static class ValidationWarningAuditExtensions
{
    public static async Task WriteValidationWarningsToAuditAsync(
        this IValidationWarningCollector warnings,
        IAuditLogService auditLog,
        Guid claimId,
        CancellationToken ct,
        Guid? correlationId = null)
    {
        foreach (var w in warnings.Warnings)
        {
            await auditLog.WriteAsync(
                claimId: claimId,
                eventType: AuditEventType.ValidationIssueAdded,
                description: $"[Warning] {w.PropertyName}: {w.Message}",
                correlationId: correlationId,
                newValue: AuditJsonValues.ValidationIssue(w.Message, "Warning"),
                ct: ct);
        }
    }
}
