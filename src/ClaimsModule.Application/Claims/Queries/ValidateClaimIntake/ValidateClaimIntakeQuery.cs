using ClaimsModule.Application.Claims.Commands.CreateClaim;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.ValidateClaimIntake;

public sealed record ValidateClaimIntakeQuery(
    Guid OrganisationId,
    Guid? PolicyId,
    string? PolicyNumber,
    string? ClientName,
    DateTimeOffset LossDate,
    string LossDescription,
    string? LossLocation,
    string CauseOfLossCode,
    decimal? EstimatedLossAmount,
    ClaimSeverity? Severity,
    string? PoliceReportNumber,
    IReadOnlyList<CreateClaimPartyInput> Parties,
    IReadOnlyList<CreateClaimRiskObjectInput> RiskObjects,
    CreateClaimInitialReserveInput? InitialReserve = null
) : IRequest<ValidateClaimIntakeResult>;
