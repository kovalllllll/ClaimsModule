using ClaimsModule.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public sealed class ApiIdempotencyRecordConfiguration : IEntityTypeConfiguration<ApiIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<ApiIdempotencyRecord> builder)
    {
        builder.ToTable("ApiIdempotencyRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ConfigureSequentialGuidKey("Id");
        builder.Property(x => x.Key).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Operation).HasMaxLength(64).IsRequired();
        builder.ConfigureAuditableColumns();

        builder.HasIndex(x => new { x.OrganisationId, x.Operation, x.Key })
            .IsUnique()
            .HasDatabaseName("UX_ApiIdempotencyRecords_Org_Operation_Key");
    }
}
