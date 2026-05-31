using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.ValueObjects;

public sealed class IdempotencyKey : ValueObject
{
    public string Value { get; }

    private IdempotencyKey(string value)
    {
        Value = value;
    }

    public static IdempotencyKey ForReserveChange(Guid reserveComponentId, int changeSequence)
    {
        if (reserveComponentId == Guid.Empty)
        {
            throw new ArgumentException("Reserve component id must not be empty.", nameof(reserveComponentId));
        }

        if (changeSequence < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(changeSequence), changeSequence, "Change sequence must be positive.");
        }

        return new IdempotencyKey($"Reserve:{reserveComponentId}:Change:{changeSequence}");
    }

    public static IdempotencyKey Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new IdempotencyKey(value);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
