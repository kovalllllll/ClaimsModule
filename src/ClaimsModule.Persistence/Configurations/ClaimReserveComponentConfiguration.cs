using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimReserveComponentConfiguration : IEntityTypeConfiguration<ClaimReserveComponent>
{
    public void Configure(EntityTypeBuilder<ClaimReserveComponent> builder)
    {
        builder.ToTable("ClaimReserveComponents");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("ReserveComponentId")
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.ClaimId).IsRequired();
        builder.Property(e => e.OrganisationId).IsRequired();

        builder.Property(e => e.Component)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<ReserveComponentType>());

        builder.Property(e => e.CurrentAmount)
            .IsRequired()
            .HasPrecision(19, 4)
            .HasConversion(
                v => v.Amount,
                d => new Money(d));

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<ReserveComponentStatus>());

        builder.Property(e => e.Notes).HasColumnType("nvarchar(max)");

        builder.Property(e => e.RowVer).IsRowVersion();

        builder.Property(e => e.CreatedAt).HasColumnType("datetimeoffset(7)").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.UserCreated);
        builder.Property(e => e.UserModified);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnType("datetimeoffset(7)");

        builder.HasOne<Claim>()
               .WithMany()
               .HasForeignKey(x => x.ClaimId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.History).WithOne().HasForeignKey(x => x.ReserveComponentId);
    }
}
