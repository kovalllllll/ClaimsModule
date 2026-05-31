using ClaimsModule.Application.Common.Interfaces;

namespace ClaimsModule.Application.Common.Services;

public sealed class ValidationWarningCollector : IValidationWarningCollector
{
    private readonly List<ValidationWarning> _warnings = new();

    public void Add(string propertyName, string message)
        => _warnings.Add(new ValidationWarning(propertyName, message));

    public IReadOnlyList<ValidationWarning> Warnings => _warnings.AsReadOnly();
}
