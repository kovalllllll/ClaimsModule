namespace ClaimsModule.Domain.Common;

public sealed class ApiIdempotencyRecord : IAuditable
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UserCreated { get; set; }
    public Guid? UserModified { get; set; }
}
