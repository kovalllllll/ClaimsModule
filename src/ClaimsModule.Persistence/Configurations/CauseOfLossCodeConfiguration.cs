using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class CauseOfLossCodeConfiguration : IEntityTypeConfiguration<CauseOfLossCode>
{
    public void Configure(EntityTypeBuilder<CauseOfLossCode> builder)
    {
        builder.ToTable("CauseOfLossCodes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ConfigureSequentialGuidKey("CauseOfLossCodeId");

        builder.Property(e => e.OrganisationId).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();

        builder.Property(e => e.PerilCategory)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<PerilCategory>());

        builder.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(e => e.SortOrder).HasDefaultValue(0).IsRequired();

        builder.ConfigureAuditableColumns();
        builder.ConfigureSoftDeleteColumns();

        builder.HasIndex(e => new { e.OrganisationId, e.Code })
            .IsUnique()
            .HasDatabaseName("UX_CauseOfLossCodes_OrgId_Code");

        var orgId = SeedConstants.SeedOrganisationId;
        var seedCreated = SeedConstants.ReferenceDataCreatedAt;

        builder.HasData(
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000001"), OrganisationId = orgId, Code = "COL-FIRE",    Name = "Fire",                    PerilCategory = PerilCategory.Property,  IsActive = true, SortOrder = 10, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000002"), OrganisationId = orgId, Code = "COL-FLOOD",   Name = "Flood",                   PerilCategory = PerilCategory.Weather,   IsActive = true, SortOrder = 20, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000003"), OrganisationId = orgId, Code = "COL-THEFT",   Name = "Theft",                   PerilCategory = PerilCategory.Crime,     IsActive = true, SortOrder = 30, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000004"), OrganisationId = orgId, Code = "COL-VEH-COL", Name = "Vehicle Collision",       PerilCategory = PerilCategory.Auto,      IsActive = true, SortOrder = 40, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000005"), OrganisationId = orgId, Code = "COL-VEH-COMP",Name = "Vehicle Comprehensive",   PerilCategory = PerilCategory.Auto,      IsActive = true, SortOrder = 50, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000006"), OrganisationId = orgId, Code = "COL-LIAB",    Name = "Third Party Liability",   PerilCategory = PerilCategory.Liability, IsActive = true, SortOrder = 60, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000007"), OrganisationId = orgId, Code = "COL-EQUIP",   Name = "Equipment Breakdown",     PerilCategory = PerilCategory.Equipment, IsActive = true, SortOrder = 70, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000008"), OrganisationId = orgId, Code = "COL-WIND",    Name = "Wind / Storm",            PerilCategory = PerilCategory.Weather,   IsActive = true, SortOrder = 80, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000009"), OrganisationId = orgId, Code = "COL-INJURY",  Name = "Bodily Injury",           PerilCategory = PerilCategory.Liability, IsActive = true, SortOrder = 90, IsDeleted = false, CreatedAt = seedCreated },
            new { Id = new Guid("bbbbbbbb-0001-0000-0000-000000000010"), OrganisationId = orgId, Code = "COL-OTHER",   Name = "Other / Unknown",         PerilCategory = PerilCategory.General,   IsActive = true, SortOrder = 100, IsDeleted = false, CreatedAt = seedCreated }
        );
    }
}
