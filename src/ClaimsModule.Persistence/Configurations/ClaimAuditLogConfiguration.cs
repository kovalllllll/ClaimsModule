using ClaimsModule.Domain.Audit;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimAuditLogConfiguration : IEntityTypeConfiguration<ClaimAuditLog>
{
    public void Configure(EntityTypeBuilder<ClaimAuditLog> builder)
    {
        builder.ToTable("ClaimAuditLog");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ConfigureSequentialGuidKey("AuditLogId");

        builder.Property(e => e.ClaimId).IsRequired();
        builder.Property(e => e.OrganisationId).IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(100)
            .IsRequired()
            .HasConversion(new AuditEventTypeValueConverter());

        builder.Property(e => e.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.OldValue).HasColumnType("nvarchar(max)");
        builder.Property(e => e.NewValue).HasColumnType("nvarchar(max)");
        builder.Property(e => e.RelatedEntityId);
        builder.Property(e => e.RelatedEntityType).HasMaxLength(255);
        builder.Property(e => e.CorrelationId);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("datetimeoffset(7)");

        builder.Property(e => e.CreatedByUserId);
        builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.UserModified);

        builder.HasOne<Claim>()
               .WithMany()
               .HasForeignKey(x => x.ClaimId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
