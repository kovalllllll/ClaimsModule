using ClaimsModule.Domain.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimSequenceConfiguration : IEntityTypeConfiguration<ClaimSequence>
{
    public void Configure(EntityTypeBuilder<ClaimSequence> builder)
    {
        builder.ToTable("ClaimSequences");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ConfigureSequentialGuidKey("SequenceId");

        builder.ConfigureAuditableColumns();

        builder.Property(e => e.OrganisationId).IsRequired();
        builder.Property(e => e.Year).IsRequired();
        builder.Property(e => e.NextValue).HasDefaultValue(1).IsRequired();

        builder.HasIndex(e => new { e.Year, e.OrganisationId })
            .IsUnique()
            .HasDatabaseName("UX_ClaimSequences_Year_OrgId");
    }
}
