namespace ClaimsModule.Application.DTOs;

public sealed class ClaimStatusTransitionDto
{
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<string> AllowedNextStatuses { get; init; } = [];
}
