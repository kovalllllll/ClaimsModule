namespace ClaimsModule.Application.Abstractions.Services;

public sealed class ClaimClosureConditionDto
{
    public required string Code { get; init; }
    public required string Description { get; init; }
    public required bool Passed { get; init; }
    public string? Detail { get; init; }
}

public sealed class ClaimClosureConditionsResult
{
    public bool CanClose { get; init; }
    public IReadOnlyList<ClaimClosureConditionDto> Conditions { get; init; } = [];
}

public interface IClaimClosureEvaluator
{
    Task<ClaimClosureConditionsResult> EvaluateAsync(
        Guid claimId,
        Guid organisationId,
        string? closureReason,
        CancellationToken cancellationToken = default);
}
