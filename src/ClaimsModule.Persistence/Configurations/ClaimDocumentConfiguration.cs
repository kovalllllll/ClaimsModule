using ClaimsModule.Domain.Documents;
using ClaimsModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimDocumentConfiguration : IEntityTypeConfiguration<ClaimDocument>
{
    public void Configure(EntityTypeBuilder<ClaimDocument> builder)
    {
        builder.ToTable("ClaimDocuments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("ClaimDocumentId")
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.ClaimId).IsRequired();
        builder.Property(e => e.OrganisationId).IsRequired();

        builder.Property(e => e.DocumentType)
            .HasMaxLength(100)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<DocumentType>());

        builder.Property(e => e.DocumentName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BlobPath).HasMaxLength(500).IsRequired();
        builder.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.FileSizeBytes).IsRequired();

        builder.Property(e => e.UploadedAt)
            .IsRequired()
            .HasColumnType("datetimeoffset(7)");

        builder.Property(e => e.UploadedByUserId);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.Property(e => e.CreatedAt).HasColumnType("datetimeoffset(7)").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset(7)");
        builder.Property(e => e.UserCreated);
        builder.Property(e => e.UserModified);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnType("datetimeoffset(7)");

    }
}
