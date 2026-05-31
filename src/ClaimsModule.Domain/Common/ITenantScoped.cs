namespace ClaimsModule.Domain.Common;

public interface ITenantScoped
{
    Guid OrganisationId { get; }
}
