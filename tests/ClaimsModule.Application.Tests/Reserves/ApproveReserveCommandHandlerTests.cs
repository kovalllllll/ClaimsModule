using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.Reserves.Commands.ApproveReserve;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Moq;

namespace ClaimsModule.Application.Tests.Reserves;

public sealed class ApproveReserveCommandHandlerTests
{
  private static readonly Guid HandlerUserId = Guid.Parse("11111111-0000-0000-0000-000000000001");
  private static readonly Guid ClaimId = Guid.NewGuid();
  private static readonly Guid ComponentId = Guid.NewGuid();
  private static readonly Guid HistoryId = Guid.NewGuid();

  [Fact]
  public async Task Handle_when_approver_is_submitter_throws_validation_exception()
  {
    var history = ReserveHistory.Create(
      ComponentId,
      ClaimId,
      Guid.Parse("00000000-0000-0000-0000-000000000001"),
      ReserveTransactionType.Add,
      new Money(50_000m),
      Money.Zero,
      Money.Zero,
      ReserveApprovalStatus.PendingApproval,
      "Test",
      IdempotencyKey.ForReserveChange(ComponentId, 1),
      1,
      HandlerUserId,
      DateTimeOffset.UtcNow);

    var reserves = new Mock<IReserveRepository>();
    reserves.Setup(r => r.GetHistoryByIdAsync(HistoryId, ClaimId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(history);

    var currentUser = new Mock<ICurrentUserService>();
    currentUser.Setup(u => u.UserId).Returns(HandlerUserId);
    currentUser.Setup(u => u.Role).Returns("supervisor");

    var handler = new ApproveReserveCommandHandler(
      Mock.Of<IClaimRepository>(),
      reserves.Object,
      Mock.Of<IUnitOfWork>(),
      Mock.Of<IAuditLogService>(),
      currentUser.Object,
      Mock.Of<ISystemClock>());

    var act = () => handler.Handle(
      new ApproveReserveCommand(HistoryId, ClaimId, Guid.Parse("00000000-0000-0000-0000-000000000001")),
      CancellationToken.None);

    var ex = await act.Should().ThrowAsync<ValidationException>();
    ex.Which.Errors.Values.SelectMany(v => v)
      .Should().Contain(m => m.Contains("Self-approval"));
  }
}
