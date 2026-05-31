using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Claims;

public sealed class ClaimSequence : Entity, ITenantScoped, IAuditable
{
    public Guid OrganisationId { get; private set; }
    public int Year { get; private set; }
    public int NextValue { get; private set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UserCreated { get; set; }
    public Guid? UserModified { get; set; }

    private ClaimSequence()
    {
    }
}
