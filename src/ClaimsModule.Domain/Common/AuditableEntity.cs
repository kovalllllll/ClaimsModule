namespace ClaimsModule.Domain.Common;

public abstract class AuditableEntity : Entity, IAuditable, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UserCreated { get; set; }
    public Guid? UserModified { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    protected AuditableEntity()
    {
    }
}
