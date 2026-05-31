using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Domain.Policies;

public sealed class Policy : Entity, ITenantScoped, IAuditable, ISoftDeletable
{
    public Guid OrganisationId { get; private set; }
    public string PolicyNumber { get; private set; } = string.Empty;
    public string ClientName { get; private set; } = string.Empty;
    public DateOnly EffectiveDate { get; private set; }
    public DateOnly ExpirationDate { get; private set; }
    public PolicyStatus Status { get; private set; }
    public string CoverageTypes { get; private set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UserCreated { get; set; }
    public Guid? UserModified { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    private Policy()
    {
    }
}
