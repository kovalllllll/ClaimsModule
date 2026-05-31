using ClaimsModule.Application.Claims;
using ClaimsModule.Domain.Enums;
using FluentAssertions;

namespace ClaimsModule.Application.Tests.Claims;

public sealed class ClaimStatusTransitionsTests
{
    [Theory]
    [InlineData(ClaimStatus.Draft, ClaimStatus.Open, true)]
    [InlineData(ClaimStatus.Draft, ClaimStatus.Closed, false)]
    [InlineData(ClaimStatus.Closed, ClaimStatus.Reopened, true)]
    [InlineData(ClaimStatus.Withdrawn, ClaimStatus.Open, false)]
    public void IsValid_returns_expected(ClaimStatus from, ClaimStatus to, bool expected)
    {
        ClaimStatusTransitions.IsValid(from, to).Should().Be(expected);
    }
}
