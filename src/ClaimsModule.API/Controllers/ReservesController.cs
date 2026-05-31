using System.Text.Json.Serialization;
using ClaimsModule.API.Serialization;
using ClaimsModule.Application.Reserves.Commands.AdjustReserveByComponent;
using ClaimsModule.Application.Reserves.Commands.ApproveReserve;
using ClaimsModule.Application.Reserves.Commands.OpenReserve;
using ClaimsModule.Application.Reserves.Commands.RejectReserve;
using ClaimsModule.Application.Reserves.Commands.RetractReserve;
using ClaimsModule.Application.Reserves.Commands.ReverseReserve;
using ClaimsModule.Application.Reserves.Commands.RetryGlPosting;
using ClaimsModule.Application.Reserves.Queries.GetClaimReserves;
using ClaimsModule.API.Extensions;
using ClaimsModule.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Authorize]
[Route("api/claims/{claimId:guid}/reserves")]
public sealed class ReservesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetReserves(Guid claimId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetClaimReservesQuery(claimId, HttpContext.GetOrganisationId()), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> OpenOrAdjust(
        Guid claimId,
        [FromBody] ReserveTransactionRequest request,
        CancellationToken ct)
    {
        var idempotencyKey = HttpContext.GetIdempotencyKey();
        var orgId = HttpContext.GetOrganisationId();

        if (request.TransactionType == ReserveTransactionType.Reverse)
        {
            var reverseResult = await mediator.Send(new ReverseReserveCommand(
                claimId, orgId, request.Component,
                request.Amount, request.ChangeReason, idempotencyKey), ct);
            return CreatedAtAction(nameof(GetReserves), new { claimId }, reverseResult);
        }

        if (request.TransactionType == ReserveTransactionType.Adjust)
        {
            var adjustResult = await mediator.Send(new AdjustReserveByComponentCommand(
                claimId, orgId, request.Component,
                request.Amount, request.ChangeReason, idempotencyKey), ct);
            return CreatedAtAction(nameof(GetReserves), new { claimId }, adjustResult);
        }

        var openResult = await mediator.Send(new OpenReserveCommand(
            claimId, orgId, request.Component,
            request.Amount, request.ChangeReason, idempotencyKey), ct);

        return CreatedAtAction(nameof(GetReserves), new { claimId }, openResult);
    }

    [HttpPost("transactions/{txnId:guid}/retry-gl")]
    public async Task<IActionResult> RetryGlPosting(Guid claimId, Guid txnId, CancellationToken ct)
    {
        await mediator.Send(new RetryGlPostingCommand(txnId, claimId, HttpContext.GetOrganisationId()), ct);
        return NoContent();
    }

    [HttpPost("{txnId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid claimId,
        Guid txnId,
        [FromBody] ApproveReserveRequest? request,
        CancellationToken ct)
    {
        await mediator.Send(new ApproveReserveCommand(
            txnId, claimId, HttpContext.GetOrganisationId(),
            request?.ManagerOverride ?? false), ct);
        return NoContent();
    }

    [HttpPost("{txnId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid claimId,
        Guid txnId,
        [FromBody] RejectReserveRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new RejectReserveCommand(
            txnId, claimId, HttpContext.GetOrganisationId(), request.RejectionReason), ct);
        return NoContent();
    }

    [HttpPost("{txnId:guid}/retract")]
    public async Task<IActionResult> Retract(Guid claimId, Guid txnId, CancellationToken ct)
    {
        await mediator.Send(new RetractReserveCommand(txnId, claimId, HttpContext.GetOrganisationId()), ct);
        return NoContent();
    }
}

public sealed class ReserveTransactionRequest
{
    [JsonConverter(typeof(StrictReserveComponentTypeJsonConverter))]
    public ReserveComponentType Component { get; init; }
    public decimal Amount { get; init; }
    public string ChangeReason { get; init; } = string.Empty;
    public ReserveTransactionType TransactionType { get; init; } = ReserveTransactionType.Add;
}

public sealed record ApproveReserveRequest(bool ManagerOverride = false);

public sealed record RejectReserveRequest(string RejectionReason);
