namespace ClaimsModule.Application.Common.Interfaces;

public sealed record ValidationWarning(string PropertyName, string Message);

public interface IValidationWarningCollector
{
    void Add(string propertyName, string message);
    IReadOnlyList<ValidationWarning> Warnings { get; }
}
