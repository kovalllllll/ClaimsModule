using ClaimsModule.Application.Claims.Commands.CreateClaim;
using ClaimsModule.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.ValidateClaimIntake;

public sealed class ValidateClaimIntakeQueryHandler(IValidator<CreateClaimCommand> createClaimValidator)
    : IRequestHandler<ValidateClaimIntakeQuery, ValidateClaimIntakeResult>
{
    private const string ClaimantField = "Parties";
    private const string ClaimantMessage = "At least one Claimant party is required to open a claim.";

    public async Task<ValidateClaimIntakeResult> Handle(
        ValidateClaimIntakeQuery request,
        CancellationToken cancellationToken)
    {
        var command = ToCreateClaimCommand(request);
        var validationResult = await createClaimValidator.ValidateAsync(command, cancellationToken);

        var issues = validationResult.Errors
            .Select(f => new ClaimIntakeValidationIssueDto(
                Severity: f.Severity == Severity.Warning ? "Warning" : "Critical",
                Field: f.PropertyName,
                Message: f.ErrorMessage))
            .ToList();

        var canCreate = !validationResult.Errors.Any(f => f.Severity != Severity.Warning);

        var hasClaimant = request.Parties.Any(p => p.PartyRole == PartyRole.Claimant);
        if (!hasClaimant)
        {
            AddIssueIfMissing(issues, ClaimantField, ClaimantMessage, "Critical");
        }

        var canOpen = canCreate && hasClaimant;

        return new ValidateClaimIntakeResult(canCreate, canOpen, issues);
    }

    private static CreateClaimCommand ToCreateClaimCommand(ValidateClaimIntakeQuery request)
        => new(
            OrganisationId: request.OrganisationId,
            PolicyId: request.PolicyId,
            PolicyNumber: request.PolicyNumber,
            ClientName: request.ClientName,
            LossDate: request.LossDate,
            LossDescription: request.LossDescription,
            LossLocation: request.LossLocation,
            CauseOfLossCode: request.CauseOfLossCode,
            EstimatedLossAmount: request.EstimatedLossAmount,
            Severity: request.Severity,
            PoliceReportNumber: request.PoliceReportNumber,
            Parties: request.Parties,
            RiskObjects: request.RiskObjects,
            InitialReserve: request.InitialReserve);

    private static void AddIssueIfMissing(
        List<ClaimIntakeValidationIssueDto> issues,
        string field,
        string message,
        string severity)
    {
        if (issues.Any(i => i.Field == field && i.Message == message))
            return;

        issues.Add(new ClaimIntakeValidationIssueDto(severity, field, message));
    }
}
