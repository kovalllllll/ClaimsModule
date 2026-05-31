using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimPartyConfiguration : IEntityTypeConfiguration<ClaimParty>
{
    public void Configure(EntityTypeBuilder<ClaimParty> builder)
    {
        builder.ToTable("ClaimParties");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("ClaimPartyId")
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.ClaimId).IsRequired();
        builder.Property(e => e.OrganisationId).IsRequired();

        builder.Property(e => e.PartyRole)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<PartyRole>());

        builder.Property(e => e.PartyType)
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<PartyType>());

        builder.Property(e => e.FirstName).HasMaxLength(100);
        builder.Property(e => e.LastName).HasMaxLength(100);
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.Email).HasMaxLength(255);
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Notes).HasColumnType("nvarchar(max)");
        builder.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();

        builder.Property(e => e.CreatedAt).HasColumnType("datetimeoffset(7)").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.UserCreated);
        builder.Property(e => e.UserModified);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnType("datetimeoffset(7)");

    }
}
