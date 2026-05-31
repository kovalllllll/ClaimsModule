namespace ClaimsModule.Domain.Common;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
    Guid? UserCreated { get; }
    Guid? UserModified { get; }
}
