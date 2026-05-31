using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ReserveHistoryConfiguration : IEntityTypeConfiguration<ReserveHistory>
{
    public void Configure(EntityTypeBuilder<ReserveHistory> builder)
    {
        builder.ToTable("ReserveHistory");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ConfigureSequentialGuidKey("ReserveHistoryId");

        builder.Property(e => e.ReserveComponentId).IsRequired();
        builder.Property(e => e.ClaimId).IsRequired();
        builder.Property(e => e.OrganisationId).IsRequired();

        builder.Property(e => e.TransactionType)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<ReserveTransactionType>());

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(19, 4)
            .HasConversion(v => v.Amount, d => new Money(d));

        builder.Property(e => e.PreviousBalance)
            .IsRequired()
            .HasPrecision(19, 4)
            .HasConversion(v => v.Amount, d => new Money(d));

        builder.Property(e => e.NewBalance)
            .IsRequired()
            .HasPrecision(19, 4)
            .HasConversion(v => v.Amount, d => new Money(d));

        builder.Property(e => e.ApprovalStatus)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<ReserveApprovalStatus>());

        builder.Property(e => e.ApprovedByUserId);
        builder.Property(e => e.ApprovedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.RejectedByUserId);
        builder.Property(e => e.RejectedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.RejectionReason).HasColumnType("nvarchar(max)");
        builder.Property(e => e.ChangeReason).HasMaxLength(500).IsRequired();

        builder.Property(e => e.PostingStatus)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<ReservePostingStatus>());

        builder.Property(e => e.PostingJobId).HasMaxLength(200);

        builder.Property(e => e.IdempotencyKey)
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                s => IdempotencyKey.Parse(s));

        builder.HasIndex(e => e.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("UX_ReserveHistory_IdempotencyKey");

        builder.Property(e => e.ChangeSequence).IsRequired();

        builder.HasIndex(e => new { e.ReserveComponentId, e.ChangeSequence })
            .IsUnique()
            .HasDatabaseName("UX_ReserveHistory_ComponentId_Sequence");

        builder.Property(e => e.SubmittedByUserId);

        builder.ConfigureAuditableColumns();
    }
}
