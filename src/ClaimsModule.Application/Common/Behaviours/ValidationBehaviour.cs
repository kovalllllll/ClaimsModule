using ClaimsModule.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using ValidationException = ClaimsModule.Application.Common.Exceptions.ValidationException;

namespace ClaimsModule.Application.Common.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    IValidationWarningCollector warningCollector)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var allFailures = (await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        // Warnings are collected in the scoped collector so handlers can read and audit them.
        // Only Error-severity failures block the request (HTTP 422).
        foreach (var warning in allFailures.Where(f => f.Severity == Severity.Warning))
            warningCollector.Add(warning.PropertyName, warning.ErrorMessage);

        var criticalFailures = allFailures
            .Where(f => f.Severity != Severity.Warning)
            .ToList();

        if (criticalFailures.Count > 0)
            throw new ValidationException(criticalFailures);

        return await next();
    }
}
