using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ConfigureSequentialGuidKey("PolicyId");

        builder.Property(e => e.OrganisationId).IsRequired();
        builder.Property(e => e.PolicyNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();

        builder.Property(e => e.EffectiveDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(e => e.ExpirationDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<PolicyStatus>());

        builder.Property(e => e.CoverageTypes)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.ConfigureAuditableColumns();
        builder.ConfigureSoftDeleteColumns();

        builder.HasIndex(e => e.PolicyNumber)
            .IsUnique()
            .HasDatabaseName("UX_Policies_PolicyNumber");

        var orgId = SeedConstants.SeedOrganisationId;
        var seedCreated = SeedConstants.ReferenceDataCreatedAt;

        builder.HasData(
            new
            {
                Id = new Guid("aaaaaaaa-0001-0000-0000-000000000001"),
                OrganisationId = orgId,
                PolicyNumber = "POL-2024-001001",
                ClientName = "Meridian Transport LLC",
                EffectiveDate = new DateOnly(2024, 1, 1),
                ExpirationDate = new DateOnly(2026, 12, 31),
                Status = PolicyStatus.Active,
                CoverageTypes = "[\"Vehicle\",\"Cargo\"]",
                IsDeleted = false,
                CreatedAt = seedCreated
            },
            new
            {
                Id = new Guid("aaaaaaaa-0001-0000-0000-000000000002"),
                OrganisationId = orgId,
                PolicyNumber = "POL-2024-001002",
                ClientName = "Harborview Properties Inc",
                EffectiveDate = new DateOnly(2024, 6, 1),
                ExpirationDate = new DateOnly(2026, 5, 31),
                Status = PolicyStatus.Active,
                CoverageTypes = "[\"Property\",\"Liability\"]",
                IsDeleted = false,
                CreatedAt = seedCreated
            },
            new
            {
                Id = new Guid("aaaaaaaa-0001-0000-0000-000000000003"),
                OrganisationId = orgId,
                PolicyNumber = "POL-2025-002001",
                ClientName = "Coastal Builders Group",
                EffectiveDate = new DateOnly(2025, 3, 1),
                ExpirationDate = new DateOnly(2027, 2, 28),
                Status = PolicyStatus.Active,
                CoverageTypes = "[\"Property\",\"Equipment\"]",
                IsDeleted = false,
                CreatedAt = seedCreated
            },
            new
            {
                Id = new Guid("aaaaaaaa-0001-0000-0000-000000000004"),
                OrganisationId = orgId,
                PolicyNumber = "POL-2025-002002",
                ClientName = "Stanton Medical Group",
                EffectiveDate = new DateOnly(2025, 1, 1),
                ExpirationDate = new DateOnly(2026, 12, 31),
                Status = PolicyStatus.Active,
                CoverageTypes = "[\"Liability\",\"Vehicle\"]",
                IsDeleted = false,
                CreatedAt = seedCreated
            },
            new
            {
                Id = new Guid("aaaaaaaa-0001-0000-0000-000000000005"),
                OrganisationId = orgId,
                PolicyNumber = "POL-2023-000099",
                ClientName = "Archived Corp",
                EffectiveDate = new DateOnly(2020, 1, 1),
                ExpirationDate = new DateOnly(2021, 12, 31),
                Status = PolicyStatus.Expired,
                CoverageTypes = "[\"Property\"]",
                IsDeleted = false,
                CreatedAt = seedCreated
            }
        );
    }
}
