using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Infrastructure.Jobs;

public sealed class SlaMonitoringJob
{
    private const string SlaBreachDescription = "Claim has not been updated in 48 hours.";

    private readonly IClaimRepository _claims;
    private readonly IAuditLogService _auditLog;
    private readonly ISystemClock _clock;

    public SlaMonitoringJob(IClaimRepository claims, IAuditLogService auditLog, ISystemClock clock)
    {
        _claims = claims;
        _auditLog = auditLog;
        _clock = clock;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var staleThreshold = _clock.UtcNow.AddHours(-48);
        var dedupThreshold = _clock.UtcNow.AddHours(-24);

        var staleClaims = await _claims.GetStaleOpenClaimsForSlaAsync(staleThreshold, ct);

        foreach (var claim in staleClaims)
        {
            var recentBreachExists = await _claims.HasRecentSlaBreachAuditAsync(claim.Id, dedupThreshold, ct);
            if (recentBreachExists)
                continue;

            await _auditLog.WriteAsync(
                claimId: claim.Id,
                eventType: AuditEventType.SlaBreachDetected,
                description: SlaBreachDescription,
                relatedEntityId: claim.Id,
                relatedEntityType: "Claim",
                ct: ct);
        }
    }
}
