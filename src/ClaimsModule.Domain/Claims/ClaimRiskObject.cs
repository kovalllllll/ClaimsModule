using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Domain.Claims;

public sealed class ClaimRiskObject : AuditableEntity
{
    public Guid ClaimId { get; private set; }
    public AssetType AssetType { get; private set; }
    public string AssetDescription { get; private set; } = string.Empty;
    public string? DamageDescription { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? AssetReference { get; private set; }

    private ClaimRiskObject() { }

    public static ClaimRiskObject Create(
        Guid claimId,
        Guid organisationId,
        AssetType assetType,
        string assetDescription,
        string? damageDescription,
        bool isPrimary,
        string? assetReference = null)
        => new()
        {
            Id = EntityId.New(),
            ClaimId = claimId,
            OrganisationId = organisationId,
            AssetType = assetType,
            AssetDescription = assetDescription,
            DamageDescription = damageDescription,
            IsPrimary = isPrimary,
            AssetReference = assetReference
        };
}
