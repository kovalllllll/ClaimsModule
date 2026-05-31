using ClaimsModule.Application.Abstractions.Services;
using FluentValidation.Results;

namespace ClaimsModule.Application.Common.Exceptions;

/// <summary>
/// FRS 10.1 PUT /status: HTTP 422 with structured blocking conditions (e.g. closure CC-01–CC-04).
/// </summary>
public sealed class StatusTransitionBlockedException : Exception
{
    public StatusTransitionBlockedException(
        IEnumerable<ValidationFailure> failures,
        IReadOnlyList<ClaimClosureConditionDto> blockingConditions)
        : base("One or more validation errors occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());

        BlockingConditions = blockingConditions;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public IReadOnlyList<ClaimClosureConditionDto> BlockingConditions { get; }
}
