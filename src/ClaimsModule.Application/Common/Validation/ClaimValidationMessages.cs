namespace ClaimsModule.Application.Common.Validation;

public static class ClaimValidationMessages
{
  public const string NoPolicyLinkedWarning =
    "No policy linked. Policy must be associated before reserves can be set.";

  public const string LossDateOutsidePolicyPeriod =
    "Loss date is outside the policy effective period.";

  public const string AggregateReserveWarning =
    "Total reserves will exceed $10,000,000. Manager override required.";

  public const string ReserveApprovalInsufficientAuthority =
    "Your role does not have authority to approve this reserve amount.";

  public const string InvalidReserveComponentType =
    "Invalid reserve component type.";
}
