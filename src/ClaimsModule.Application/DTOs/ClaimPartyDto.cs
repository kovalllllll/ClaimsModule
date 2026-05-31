namespace ClaimsModule.Application.DTOs;

public sealed class ClaimPartyDto
{
    public Guid Id { get; init; }
    public string PartyRole { get; init; } = string.Empty;
    public string PartyType { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
}
