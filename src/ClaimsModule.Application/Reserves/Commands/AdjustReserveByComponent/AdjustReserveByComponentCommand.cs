using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Reserves.Commands.AdjustReserve;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Reserves.Commands.AdjustReserveByComponent;

public sealed record AdjustReserveByComponentCommand(
    Guid ClaimId,
    Guid OrganisationId,
    ReserveComponentType Component,
    decimal Amount,
    string ChangeReason,
    string? IdempotencyKey
) : ICommand<AdjustReserveResult>;
