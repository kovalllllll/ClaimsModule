using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Common.Behaviours;

public sealed class UnitOfWorkBehaviour<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICommand and not ICommand<TResponse>)
            return await next();

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next();
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
