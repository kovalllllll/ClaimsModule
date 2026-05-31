using ClaimsModule.Application.Policies.Queries.GetPolicyCoverage;
using ClaimsModule.Application.Policies.Queries.SearchPolicies;
using ClaimsModule.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Authorize]
[Route("api/policies")]
public sealed class PoliciesController(IMediator mediator) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct)
    {
        var result = await mediator.Send(
            new SearchPoliciesQuery(HttpContext.GetOrganisationId(), q ?? string.Empty), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/coverage")]
    public async Task<IActionResult> GetCoverage(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetPolicyCoverageQuery(id, HttpContext.GetOrganisationId()), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
