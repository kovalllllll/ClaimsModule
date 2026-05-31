using AutoMapper;
using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimAudit;

public sealed class GetClaimAuditQueryHandler(IClaimRepository claims, IMapper mapper)
    : IRequestHandler<GetClaimAuditQuery, PagedResult<AuditLogEntryDto>>
{
    public async Task<PagedResult<AuditLogEntryDto>> Handle(
        GetClaimAuditQuery request,
        CancellationToken cancellationToken)
    {
        var (entries, totalCount) = await claims.GetAuditPagedAsync(
            request.ClaimId,
            request.OrganisationId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = entries.Select(mapper.Map<AuditLogEntryDto>).ToList();
        return PagedResult<AuditLogEntryDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}
