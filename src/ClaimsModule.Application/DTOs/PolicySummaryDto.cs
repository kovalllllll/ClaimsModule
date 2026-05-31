namespace ClaimsModule.Application.DTOs;

public sealed class PolicySummaryDto
{
    public Guid Id { get; init; }
    public string PolicyNumber { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public DateOnly EffectiveDate { get; init; }
    public DateOnly ExpirationDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<string> CoverageTypes { get; init; } = [];
}
