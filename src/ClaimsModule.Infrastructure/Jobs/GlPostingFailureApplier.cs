using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Jobs;

public static class GlPostingFailureApplier
{
    public static void Apply(IServiceScopeFactory scopeFactory, Guid reserveHistoryId, string failureReason)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var auditLog = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

        var history = db.ReserveHistory.FirstOrDefault(h => h.Id == reserveHistoryId);
        if (history is null || history.PostingStatus == ReservePostingStatus.Posted)
            return;

        history.MarkPostingFailed();
        db.SaveChanges();

        auditLog.WriteAsync(
            claimId: history.ClaimId,
            eventType: AuditEventType.GlPostingFailed,
            description: $"GL reserve posting failed for history {reserveHistoryId}.",
            newValue: AuditJsonValues.FailureReason(failureReason),
            relatedEntityId: history.Id,
            relatedEntityType: "ReserveHistory").GetAwaiter().GetResult();
    }
}
