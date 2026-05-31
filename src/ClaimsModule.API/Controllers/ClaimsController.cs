using ClaimsModule.Application.Claims.Commands.AddClaimParty;
using ClaimsModule.Application.Claims.Commands.CreateClaim;
using ClaimsModule.Application.Claims.Commands.LinkPolicy;
using ClaimsModule.Application.Claims.Commands.RemoveClaimParty;
using ClaimsModule.Application.Claims.Commands.TransitionClaimStatus;
using ClaimsModule.Application.Claims.Commands.UpdateClaimNotes;
using ClaimsModule.Application.Claims.Queries.GetClaimAudit;
using ClaimsModule.Application.Claims.Queries.GetClaimClosureConditions;
using ClaimsModule.Application.Claims.Queries.GetClaimDetail;
using ClaimsModule.Application.Claims.Queries.ListClaims;
using ClaimsModule.Application.Claims.Queries.ValidateClaimIntake;
using ClaimsModule.Application.Documents.Commands.UploadClaimDocument;
using ClaimsModule.Application.Documents.Queries.GetClaimDocuments;
using ClaimsModule.API.Extensions;
using ClaimsModule.API.Serialization;
using System.Text.Json.Serialization;
using ClaimsModule.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Authorize]
[Route("api/claims")]
public sealed class ClaimsController(IMediator mediator) : ControllerBase
{
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] CreateClaimRequest request, CancellationToken ct)
    {
        var orgId = HttpContext.GetOrganisationId();
        var result = await mediator.Send(MapValidateQuery(orgId, request), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClaimRequest request, CancellationToken ct)
    {
        var orgId = HttpContext.GetOrganisationId();
        var command = MapCreateCommand(
            orgId,
            request,
            HttpContext.GetCorrelationId(),
            HttpContext.GetIdempotencyKey());

        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.ClaimId }, result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] ClaimStatus? status,
        [FromQuery] ClaimStatus[]? statuses,
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        [FromQuery] Guid? assignedHandlerId,
        [FromQuery] string? assignedHandlerSearch,
        [FromQuery] string? causeOfLossCode,
        [FromQuery] Guid? policyId,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListClaimsQuery(
            HttpContext.GetOrganisationId(),
            status,
            statuses,
            dateFrom,
            dateTo,
            assignedHandlerId,
            assignedHandlerSearch,
            causeOfLossCode,
            policyId,
            search,
            pageNumber,
            pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetClaimDetailQuery(id, HttpContext.GetOrganisationId()), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/closure-conditions")]
    public async Task<IActionResult> GetClosureConditions(
        Guid id,
        [FromQuery] string? reason,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetClaimClosureConditionsQuery(id, HttpContext.GetOrganisationId(), reason), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/policy")]
    public async Task<IActionResult> LinkPolicy(
        Guid id,
        [FromBody] LinkPolicyRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new LinkPolicyCommand(
            id, HttpContext.GetOrganisationId(), request.PolicyId), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/notes")]
    public async Task<IActionResult> UpdateNotes(
        Guid id,
        [FromBody] UpdateClaimNotesRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new UpdateClaimNotesCommand(
            id, HttpContext.GetOrganisationId(), request.Notes), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> TransitionStatus(
        Guid id,
        [FromBody] TransitionStatusRequest request,
        CancellationToken ct)
    {
        var rowVer = Request.Headers.IfMatch.FirstOrDefault();
        await mediator.Send(new TransitionClaimStatusCommand(
            id, HttpContext.GetOrganisationId(), request.TargetStatus, request.Reason, rowVer), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<IActionResult> GetAudit(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetClaimAuditQuery(id, HttpContext.GetOrganisationId(), pageNumber, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/parties")]
    public async Task<IActionResult> AddParty(Guid id, [FromBody] AddPartyRequest request, CancellationToken ct)
    {
        var partyId = await mediator.Send(new AddClaimPartyCommand(
            id, HttpContext.GetOrganisationId(),
            request.PartyRole, request.PartyType,
            request.FirstName, request.LastName, request.CompanyName,
            request.Email, request.Phone, request.Notes), ct);
        return Created($"/api/claims/{id}/parties/{partyId}", new { partyId });
    }

    [HttpDelete("{id:guid}/parties/{partyId:guid}")]
    public async Task<IActionResult> RemoveParty(Guid id, Guid partyId, CancellationToken ct)
    {
        await mediator.Send(new RemoveClaimPartyCommand(id, partyId, HttpContext.GetOrganisationId()), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/documents")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> UploadDocument(
        Guid id,
        [FromForm] UploadDocumentRequest request,
        CancellationToken ct)
    {
        await using var stream = request.File.OpenReadStream();
        var documentId = await mediator.Send(new UploadClaimDocumentCommand(
            id,
            HttpContext.GetOrganisationId(),
            request.DocumentType,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            stream,
            request.Notes,
            HttpContext.GetIdempotencyKey()), ct);

        return Created($"/api/claims/{id}/documents/{documentId}", new { documentId });
    }

    [HttpGet("{id:guid}/documents")]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetClaimDocumentsQuery(id, HttpContext.GetOrganisationId()), ct);
        return Ok(result);
    }

    private static ValidateClaimIntakeQuery MapValidateQuery(Guid orgId, CreateClaimRequest request)
        => new(
            OrganisationId: orgId,
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
            Parties: MapParties(request.Parties),
            RiskObjects: MapRiskObjects(request.RiskObjects),
            InitialReserve: MapInitialReserve(request.InitialReserve));

    private static CreateClaimCommand MapCreateCommand(
        Guid orgId,
        CreateClaimRequest request,
        Guid? correlationId,
        string? idempotencyKey)
        => new(
            OrganisationId: orgId,
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
            Parties: MapParties(request.Parties),
            RiskObjects: MapRiskObjects(request.RiskObjects),
            InitialReserve: MapInitialReserve(request.InitialReserve),
            CorrelationId: correlationId,
            IdempotencyKey: idempotencyKey);

    private static List<CreateClaimPartyInput> MapParties(List<CreateClaimPartyRequest> parties)
        => parties.Select(p => new CreateClaimPartyInput(
            p.PartyRole, p.PartyType, p.FirstName, p.LastName,
            p.CompanyName, p.Email, p.Phone, p.Notes)).ToList();

    private static List<CreateClaimRiskObjectInput> MapRiskObjects(List<CreateClaimRiskObjectRequest> riskObjects)
        => riskObjects.Select(r => new CreateClaimRiskObjectInput(
            r.AssetType, r.AssetDescription, r.DamageDescription,
            r.IsPrimary, r.AssetReference)).ToList();

    private static CreateClaimInitialReserveInput? MapInitialReserve(
        CreateClaimInitialReserveRequest? initialReserve)
        => initialReserve is null
            ? null
            : new CreateClaimInitialReserveInput(
                initialReserve.ComponentType,
                initialReserve.Amount,
                initialReserve.ChangeReason);
}

public sealed class CreateClaimRequest
{
    public Guid? PolicyId { get; init; }
    public string? PolicyNumber { get; init; }
    public string? ClientName { get; init; }
    public DateTimeOffset LossDate { get; init; }
    public string LossDescription { get; init; } = string.Empty;
    public string? LossLocation { get; init; }
    public string CauseOfLossCode { get; init; } = string.Empty;
    public decimal? EstimatedLossAmount { get; init; }
    public ClaimSeverity? Severity { get; init; }
    public string? PoliceReportNumber { get; init; }
    public List<CreateClaimPartyRequest> Parties { get; init; } = [];
    public List<CreateClaimRiskObjectRequest> RiskObjects { get; init; } = [];
    public CreateClaimInitialReserveRequest? InitialReserve { get; init; }
}

public sealed class CreateClaimPartyRequest
{
    public PartyRole PartyRole { get; init; }
    public PartyType PartyType { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
}

public sealed class CreateClaimRiskObjectRequest
{
    public AssetType AssetType { get; init; }
    public string AssetDescription { get; init; } = string.Empty;
    public string? DamageDescription { get; init; }
    public bool IsPrimary { get; init; }
    public string? AssetReference { get; init; }
}

public sealed class CreateClaimInitialReserveRequest
{
    [JsonConverter(typeof(StrictReserveComponentTypeJsonConverter))]
    public ReserveComponentType ComponentType { get; init; }
    public decimal Amount { get; init; }
    public string ChangeReason { get; init; } = string.Empty;
}

public sealed record TransitionStatusRequest(ClaimStatus TargetStatus, string? Reason);

public sealed record LinkPolicyRequest(Guid PolicyId);

public sealed record UpdateClaimNotesRequest(string? Notes);

public sealed class AddPartyRequest
{
    public PartyRole PartyRole { get; init; }
    public PartyType PartyType { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
}

public sealed class UploadDocumentRequest
{
    public DocumentType DocumentType { get; init; }
    public IFormFile File { get; init; } = null!;
    public string? Notes { get; init; }
}
