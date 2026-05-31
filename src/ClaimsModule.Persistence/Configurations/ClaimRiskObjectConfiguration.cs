using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimRiskObjectConfiguration : IEntityTypeConfiguration<ClaimRiskObject>
{
    public void Configure(EntityTypeBuilder<ClaimRiskObject> builder)
    {
        builder.ToTable("ClaimRiskObjects");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("ClaimRiskObjectId")
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.ClaimId).IsRequired();
        builder.Property(e => e.OrganisationId).IsRequired();

        builder.Property(e => e.AssetType)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<AssetType>());

        builder.Property(e => e.AssetDescription)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.DamageDescription).HasColumnType("nvarchar(max)");
        builder.Property(e => e.IsPrimary).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.AssetReference).HasMaxLength(255);

        builder.Property(e => e.CreatedAt).HasColumnType("datetimeoffset(7)").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.UserCreated);
        builder.Property(e => e.UserModified);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnType("datetimeoffset(7)");

    }
}
