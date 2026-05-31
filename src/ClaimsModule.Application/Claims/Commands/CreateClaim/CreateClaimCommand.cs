using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Claims.Commands.CreateClaim;

public sealed record CreateClaimCommand(
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
    CreateClaimInitialReserveInput? InitialReserve = null,
    Guid? CorrelationId = null,
    string? IdempotencyKey = null
) : ICommand<CreateClaimResult>;

public sealed record CreateClaimPartyInput(
    PartyRole PartyRole,
    PartyType PartyType,
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? Email,
    string? Phone,
    string? Notes = null
);

public sealed record CreateClaimRiskObjectInput(
    AssetType AssetType,
    string AssetDescription,
    string? DamageDescription,
    bool IsPrimary,
    string? AssetReference = null
);

/// <summary>
/// Optional initial reserve component to create atomically with the claim.
/// If PolicyId is null the reserve will be skipped and a warning returned (BR-C-06).
/// </summary>
public sealed record CreateClaimInitialReserveInput(
    ReserveComponentType ComponentType,
    decimal Amount,
    string ChangeReason
);

/// <summary>
/// Returns the created claim identity and any non-blocking warnings that must be
/// acknowledged before the claim can be transitioned to Open.
/// </summary>
public sealed record CreateClaimResult(
    Guid ClaimId,
    string ClaimNumber,
    IReadOnlyList<string> Warnings
);
