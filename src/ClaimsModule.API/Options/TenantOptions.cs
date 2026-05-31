namespace ClaimsModule.API.Options;

public sealed class TenantOptions
{
    public const string SectionName = "Tenant";

    public Guid DefaultOrganisationId { get; set; }
}
