using System.Text.RegularExpressions;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.ValueObjects;

public sealed partial class ClaimNumber : ValueObject
{
    private const string FormatPattern = @"^CLM-\d{4}-\d{7}$";

    public string Value { get; }

    private ClaimNumber(string value)
    {
        Value = value;
    }

    public static ClaimNumber Create(int year, int sequence)
    {
        if (year is < 1900 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be a 4-digit value.");
        }

        if (sequence is < 1 or > 9_999_999)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Sequence must be a positive 7-digit value.");
        }

        return new ClaimNumber($"CLM-{year:D4}-{sequence:D7}");
    }

    public static ClaimNumber Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!FormatRegex().IsMatch(value))
        {
            throw new FormatException($"Claim number '{value}' does not match required format 'CLM-YYYY-NNNNNNN'.");
        }

        return new ClaimNumber(value);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(FormatPattern)]
    private static partial Regex FormatRegex();
}
