namespace ClaimsModule.Application.DTOs;

public sealed class ClaimRiskObjectDto
{
    public Guid Id { get; init; }
    public string AssetType { get; init; } = string.Empty;
    public string AssetDescription { get; init; } = string.Empty;
    public string? DamageDescription { get; init; }
    public bool IsPrimary { get; init; }
    public string? AssetReference { get; init; }
}
