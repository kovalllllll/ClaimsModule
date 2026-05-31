namespace ClaimsModule.Application.Abstractions.Services;

public interface ICorrelationIdAccessor
{
    Guid? CorrelationId { get; }
}
