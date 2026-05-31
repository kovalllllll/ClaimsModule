using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class LossEventConfiguration : IEntityTypeConfiguration<LossEvent>
{
    public void Configure(EntityTypeBuilder<LossEvent> builder)
    {
        builder.ToTable("LossEvents");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("LossEventId")
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.ClaimId).IsRequired();
        builder.Property(e => e.OrganisationId).IsRequired();

        builder.Property(e => e.LossDate)
            .IsRequired()
            .HasColumnType("datetimeoffset(7)");

        builder.Property(e => e.LossDescription)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.LossLocation).HasMaxLength(500);

        builder.Property(e => e.CauseOfLossCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EstimatedLossAmount)
            .HasPrecision(19, 4)
            .HasConversion(
                v => v == null ? (decimal?)null : v.Amount,
                d => d.HasValue ? new Money(d.Value) : null);

        builder.Property(e => e.ReportDate)
            .IsRequired()
            .HasColumnType("datetimeoffset(7)");

        builder.Property(e => e.PoliceReportNumber).HasMaxLength(100);

        builder.Property(e => e.CreatedAt).HasColumnType("datetimeoffset(7)").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.UserCreated);
        builder.Property(e => e.UserModified);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnType("datetimeoffset(7)");

    }
}
