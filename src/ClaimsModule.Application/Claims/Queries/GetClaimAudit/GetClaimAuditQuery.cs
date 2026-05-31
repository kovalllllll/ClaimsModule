using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimAudit;

public sealed record GetClaimAuditQuery(
    Guid ClaimId,
    Guid OrganisationId,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<PagedResult<AuditLogEntryDto>>;
