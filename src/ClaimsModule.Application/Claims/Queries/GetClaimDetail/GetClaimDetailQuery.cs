using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimDetail;

public sealed record GetClaimDetailQuery(Guid ClaimId, Guid OrganisationId) : IRequest<ClaimDetailDto?>;
