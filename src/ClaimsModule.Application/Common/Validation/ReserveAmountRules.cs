using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Common.Validation;

/// <summary>
/// BR-R-01 and FRS 6.2 reserve component amount rules.
/// </summary>
public static class ReserveAmountRules
{
    public const string TransactionAmountMessage =
        "Reserve amount must be greater than zero, except Subrogation Recoverable which may be negative (but not zero).";

    public const string NonNegativeBalanceMessage =
        "Indemnity, Expense, and ALAE reserve balances cannot be negative.";

    /// <summary>
    /// Validates a single reserve transaction amount (Add/Open/initial).
    /// </summary>
    public static bool IsValidTransactionAmount(ReserveComponentType componentType, decimal amount)
    {
        if (amount == 0m)
        {
            return false;
        }

        if (componentType == ReserveComponentType.SubrogationRecoverable)
        {
            return true;
        }

        return amount > 0m;
    }

    /// <summary>
    /// FRS 6.2: only SubrogationRecoverable may have a negative running balance.
    /// </summary>
    public static bool ViolatesNonNegativeBalanceRule(ReserveComponentType componentType, decimal balance)
    {
        if (componentType == ReserveComponentType.SubrogationRecoverable)
        {
            return false;
        }

        return balance < 0m;
    }
}
