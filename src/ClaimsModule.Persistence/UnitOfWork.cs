using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClaimsModule.Persistence;

public sealed class UnitOfWork(ClaimsDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(
                "The record was modified by another user. Refresh and try again.", ex);
        }
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfDbTransaction(tx);
    }
}

internal sealed class EfDbTransaction : IDbTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfDbTransaction(IDbContextTransaction transaction) => _transaction = transaction;

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => _transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => _transaction.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync() => _transaction.DisposeAsync();
}
