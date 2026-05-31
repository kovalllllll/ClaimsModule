using ClaimsModule.API.Tests.Support;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.ValueObjects;
using ClaimsModule.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.API.Tests;

public sealed class DataConventionsIntegrationTests : IClassFixture<ClaimsApiFactory>
{
    private readonly ClaimsApiFactory _factory;

    public DataConventionsIntegrationTests(ClaimsApiFactory factory) => _factory = factory;

    [Fact]
    public void Global_soft_delete_filter_hides_deleted_claims()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        var claim = Claim.Create(
            organisationId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            claimNumber: ClaimNumber.Parse("CLM-2026-9999999"),
            policyId: Guid.Parse("aaaaaaaa-0001-0000-0000-000000000001"),
            policyNumber: "POL-2024-001001",
            clientName: "Test Client",
            severity: ClaimSeverity.Standard,
            reportedDate: DateTimeOffset.UtcNow);

        db.Claims.Add(claim);
        db.SaveChanges();

        db.Claims.Count().Should().Be(1);

        claim.IsDeleted = true;
        claim.DeletedAt = DateTimeOffset.UtcNow;
        db.SaveChanges();

        db.Claims.Count().Should().Be(0);
        db.Claims.IgnoreQueryFilters().Count().Should().Be(1);
    }

    [Fact]
    public void Seeded_policies_have_audit_and_soft_delete_columns()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        var policy = db.Policies
            .FirstOrDefault(p => p.PolicyNumber == "POL-2024-001001");

        policy.Should().NotBeNull();
        policy!.IsDeleted.Should().BeFalse();
        policy.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Created_claim_uses_sequential_friendly_entity_id()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        var claim = Claim.Create(
            organisationId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            claimNumber: ClaimNumber.Parse("CLM-2026-8888888"),
            policyId: null,
            policyNumber: null,
            clientName: null,
            severity: null,
            reportedDate: DateTimeOffset.UtcNow);

        var version = (claim.Id.ToByteArray()[7] >> 4) & 0x0F;
        version.Should().Be(7);
    }
}
