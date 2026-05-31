using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Reserves.Commands.AdjustReserve;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.AdjustReserveByComponent;

public sealed class AdjustReserveByComponentCommandHandler(IReserveRepository reserves, IMediator mediator)
    : IRequestHandler<AdjustReserveByComponentCommand, AdjustReserveResult>
{
    public async Task<AdjustReserveResult> Handle(
        AdjustReserveByComponentCommand request,
        CancellationToken cancellationToken)
    {
        var component = await reserves.GetComponentByTypeAsync(
                request.ClaimId, request.OrganisationId, request.Component, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Reserve component {request.Component} not found for claim {request.ClaimId}.");

        return await mediator.Send(new AdjustReserveCommand(
            component.Id,
            request.ClaimId,
            request.OrganisationId,
            request.Amount,
            request.ChangeReason,
            request.IdempotencyKey), cancellationToken);
    }
}
