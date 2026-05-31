using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Reserves.Queries.GetClaimReserves;

public sealed record GetClaimReservesQuery(Guid ClaimId, Guid OrganisationId)
    : IRequest<ReserveDetailDto?>;
