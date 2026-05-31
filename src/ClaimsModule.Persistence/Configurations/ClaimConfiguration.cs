using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ConfigureSequentialGuidKey("ClaimId");

        builder.Property(e => e.ClaimNumber)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                s => ClaimNumber.Parse(s));

        builder.HasIndex(e => new { e.OrganisationId, e.ClaimNumber })
            .IsUnique()
            .HasDatabaseName("UX_Claims_OrgId_ClaimNumber");

        builder.Property(e => e.OrganisationId).IsRequired();

        builder.Property(e => e.PolicyId);
        builder.Property(e => e.PolicyNumber).HasMaxLength(50);
        builder.Property(e => e.ClientName).HasMaxLength(255);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<ClaimStatus>());

        builder.Property(e => e.Severity)
            .HasMaxLength(50)
            .HasConversion(new EnumToStringConverter<ClaimSeverity>());

        builder.Property(e => e.ReportedDate)
            .IsRequired()
            .HasColumnType("datetimeoffset(7)");

        builder.Property(e => e.AssignedHandlerId);

        builder.Property(e => e.ClosedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.ClosureReason).HasColumnType("nvarchar(max)");
        builder.Property(e => e.Notes).HasColumnType("nvarchar(max)");
        builder.Property(e => e.ManagerOverrideFlag).HasDefaultValue(false).IsRequired();

        builder.Property(e => e.RowVer).IsRowVersion();

        builder.Property(e => e.CreatedAt).HasColumnType("datetimeoffset(7)").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.UserCreated);
        builder.Property(e => e.UserModified);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnType("datetimeoffset(7)");

        builder.HasMany(e => e.LossEvents).WithOne().HasForeignKey(x => x.ClaimId);
        builder.HasMany(e => e.Parties).WithOne().HasForeignKey(x => x.ClaimId);
        builder.HasMany(e => e.RiskObjects).WithOne().HasForeignKey(x => x.ClaimId);
        builder.HasMany(e => e.Documents).WithOne().HasForeignKey(x => x.ClaimId);
    }
}
