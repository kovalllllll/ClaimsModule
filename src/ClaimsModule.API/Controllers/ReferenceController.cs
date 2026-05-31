using ClaimsModule.Application.Claims.Queries.GetClaimStatuses;
using ClaimsModule.Application.Policies.Queries.GetCauseOfLossCodes;
using ClaimsModule.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Authorize]
[Route("api/reference")]
public sealed class ReferenceController(IMediator mediator) : ControllerBase
{
    [HttpGet("cause-of-loss-codes")]
    public async Task<IActionResult> GetCauseOfLossCodes(
        [FromQuery] string? perilCategory,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetCauseOfLossCodesQuery(HttpContext.GetOrganisationId(), perilCategory), ct);
        return Ok(result);
    }

    [HttpGet("claim-statuses")]
    public async Task<IActionResult> GetClaimStatuses(CancellationToken ct)
    {
        var result = await mediator.Send(new GetClaimStatusesQuery(), ct);
        return Ok(result);
    }
}
