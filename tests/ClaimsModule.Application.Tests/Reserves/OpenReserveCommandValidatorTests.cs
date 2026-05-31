using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common.Validation;
using ClaimsModule.Application.Reserves.Commands.OpenReserve;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.ValueObjects;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ClaimsModule.Application.Tests.Reserves;

public sealed class OpenReserveCommandValidatorTests
{
  [Fact]
  public async Task Validate_when_projected_total_exceeds_threshold_emits_warning_not_error()
  {
    var claimId = Guid.NewGuid();
    var orgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    var reserves = new Mock<IReserveRepository>();
    var component = ClaimReserveComponent.Create(claimId, orgId, ReserveComponentType.Indemnity);
    component.UpdateCurrentAmount(new Money(9_500_000m));
    reserves.Setup(r => r.GetComponentsByClaimIdAsync(claimId, It.IsAny<CancellationToken>()))
      .ReturnsAsync([component]);

    var validationQueries = new Mock<IValidationQueries>();
    validationQueries.Setup(v => v.ClaimHasLinkedPolicyAsync(claimId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);
    validationQueries.Setup(v => v.HasPendingApprovalForComponentTypeAsync(
        claimId, ReserveComponentType.Expense, It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    var validator = new OpenReserveCommandValidator(validationQueries.Object, reserves.Object);
    var command = new OpenReserveCommand(
      claimId, orgId, ReserveComponentType.Expense, 1_000_001m, "Test", "key-1");

    var result = await validator.ValidateAsync(command);

    result.Errors.Where(e => e.Severity != Severity.Warning).Should().BeEmpty();
    result.Errors.Should().ContainSingle(e =>
      e.Severity == Severity.Warning
      && e.ErrorMessage == ClaimValidationMessages.AggregateReserveWarning);
  }
}
