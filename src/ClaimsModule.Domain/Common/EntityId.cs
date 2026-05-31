namespace ClaimsModule.Domain.Common;

/// <summary>
/// Generates sequential-friendly GUIDs (RFC 9562 v7) for client-assigned keys before SaveChanges.
/// Database inserts without an explicit Id still use NEWSEQUENTIALID() per FRS 15.
/// </summary>
public static class EntityId
{
    public static Guid New() => Guid.CreateVersion7();
}
