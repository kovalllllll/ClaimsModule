using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Domain.Parties;

public sealed class ClaimParty : AuditableEntity
{
    public Guid ClaimId { get; private set; }
    public PartyRole PartyRole { get; private set; }
    public PartyType PartyType { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ClaimParty() { }

    public static ClaimParty Create(
        Guid claimId,
        Guid organisationId,
        PartyRole partyRole,
        PartyType partyType,
        string? firstName,
        string? lastName,
        string? companyName,
        string? email,
        string? phone,
        string? notes = null)
        => new()
        {
            Id = EntityId.New(),
            ClaimId = claimId,
            OrganisationId = organisationId,
            PartyRole = partyRole,
            PartyType = partyType,
            FirstName = firstName,
            LastName = lastName,
            CompanyName = companyName,
            Email = email,
            Phone = phone,
            Notes = notes,
            IsActive = true
        };

    public void Deactivate() => IsActive = false;
}
