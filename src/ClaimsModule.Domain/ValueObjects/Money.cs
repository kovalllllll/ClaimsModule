using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public static Money Zero { get; } = new(0m);

    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = decimal.Round(amount, 4, MidpointRounding.AwayFromZero);
    }

    public static Money From(decimal amount) => new(amount);

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Money(Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Money(Amount - other.Amount);
    }

    public override string ToString() => Amount.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }
}
