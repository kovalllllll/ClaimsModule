namespace ClaimsModule.Application.Abstractions.Persistence;

public interface IClaimNumberGenerator
{
    Task<int> AllocateNextSequenceAsync(Guid organisationId, int year, CancellationToken cancellationToken = default);
}
