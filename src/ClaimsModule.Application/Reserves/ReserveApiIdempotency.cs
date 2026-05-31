using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Reserves.Commands.AdjustReserve;
using ClaimsModule.Application.Reserves.Commands.OpenReserve;
using ClaimsModule.Application.Reserves.Commands.ReverseReserve;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Application.Reserves;

public static class ReserveApiIdempotencyOperations
{
    public const string OpenReserve = "OpenReserve";
    public const string AdjustReserve = "AdjustReserve";
    public const string ReverseReserve = "ReverseReserve";
}

public sealed class ReserveApiIdempotency(
    IApiIdempotencyRepository idempotency,
    IReserveRepository reserves,
    ISystemClock clock)
{
    public async Task<Guid?> TryGetCachedHistoryIdAsync(
        Guid organisationId,
        string operation,
        string? httpKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(httpKey))
            return null;

        var existing = await idempotency.FindAsync(
            organisationId, operation, httpKey, cancellationToken);

        return existing?.ResourceId;
    }

    public async Task RecordAsync(
        Guid organisationId,
        string operation,
        string? httpKey,
        Guid reserveHistoryId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(httpKey))
            return;

        await idempotency.AddAsync(new ApiIdempotencyRecord
        {
            Id = EntityId.New(),
            OrganisationId = organisationId,
            Key = httpKey,
            Operation = operation,
            ResourceId = reserveHistoryId,
            CreatedAt = clock.UtcNow
        }, cancellationToken);
    }

    public async Task<ReserveHistory> GetHistoryOrThrowAsync(
        Guid reserveHistoryId,
        CancellationToken cancellationToken)
    {
        return await reserves.GetHistoryByIdOnlyAsync(reserveHistoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Reserve history {reserveHistoryId} not found.");
    }

    public static OpenReserveResult ToOpenResult(ReserveHistory history) =>
        new(history.Id, history.ApprovalStatus.ToString(), IsAutoOrApproved(history));

    public static AdjustReserveResult ToAdjustResult(ReserveHistory history) =>
        new(history.Id, history.ApprovalStatus.ToString(), IsAutoOrApproved(history));

    public static ReverseReserveResult ToReverseResult(ReserveHistory history) =>
        new(history.Id, history.ApprovalStatus.ToString(), IsAutoOrApproved(history));

    private static bool IsAutoOrApproved(ReserveHistory history) =>
        history.ApprovalStatus is ReserveApprovalStatus.AutoApproved
            or ReserveApprovalStatus.Approved;
}
