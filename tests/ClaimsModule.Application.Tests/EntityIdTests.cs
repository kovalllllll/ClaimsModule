using ClaimsModule.Domain.Common;
using FluentAssertions;

namespace ClaimsModule.Application.Tests;

public sealed class EntityIdTests
{
    [Fact]
    public void New_returns_uuid_version_7()
    {
        var id = EntityId.New();
        var version = (id.ToByteArray()[7] >> 4) & 0x0F;
        version.Should().Be(7);
    }

    [Fact]
    public void New_generates_distinct_values()
    {
        EntityId.New().Should().NotBe(EntityId.New());
    }
}
