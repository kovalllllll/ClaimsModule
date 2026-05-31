using ClaimsModule.Application.Abstractions.Persistence;

namespace ClaimsModule.API.Tests.Support;

internal sealed class TestClaimNumberGenerator : IClaimNumberGenerator
{
  private static int _sequence;

  public Task<int> AllocateNextSequenceAsync(
    Guid organisationId,
    int year,
    CancellationToken cancellationToken = default)
    => Task.FromResult(Interlocked.Increment(ref _sequence));
}
