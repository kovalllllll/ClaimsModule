using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Domain.Audit;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Infrastructure.Persistence;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ISystemClock _clock;
    private readonly ICorrelationIdAccessor _correlationId;

    public AuditLogService(
        IClaimRepository claims,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ISystemClock clock,
        ICorrelationIdAccessor correlationId)
    {
        _claims = claims;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _correlationId = correlationId;
    }

    public async Task WriteAsync(
        Guid claimId,
        AuditEventType eventType,
        string description,
        Guid? correlationId = null,
        string? oldValue = null,
        string? newValue = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken ct = default)
    {
        var organisationId = await _claims.GetOrganisationIdAsync(claimId, ct) ?? Guid.Empty;

        var entry = ClaimAuditLog.Create(
            claimId: claimId,
            organisationId: organisationId,
            eventType: eventType,
            description: description,
            createdByUserId: _currentUser.UserId,
            createdAt: _clock.UtcNow,
            correlationId: correlationId ?? _correlationId.CorrelationId,
            oldValue: oldValue,
            newValue: newValue,
            relatedEntityId: relatedEntityId,
            relatedEntityType: relatedEntityType);

        await _claims.AddAuditEntryAsync(entry, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
