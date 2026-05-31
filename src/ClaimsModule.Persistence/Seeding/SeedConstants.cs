namespace ClaimsModule.Persistence.Seeding;

public static class SeedConstants
{
    public static readonly Guid SeedOrganisationId = new("00000000-0000-0000-0000-000000000001");

    public static readonly DateTimeOffset ReferenceDataCreatedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
