namespace ClaimsModule.Application.DTOs;

public sealed class CauseOfLossCodeDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string PerilCategory { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}
