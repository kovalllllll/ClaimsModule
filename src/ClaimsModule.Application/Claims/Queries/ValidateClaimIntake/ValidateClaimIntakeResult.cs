namespace ClaimsModule.Application.Claims.Queries.ValidateClaimIntake;

public sealed record ClaimIntakeValidationIssueDto(
    string Severity,
    string Field,
    string Message);

public sealed record ValidateClaimIntakeResult(
    bool CanCreate,
    bool CanOpen,
    IReadOnlyList<ClaimIntakeValidationIssueDto> Issues);
