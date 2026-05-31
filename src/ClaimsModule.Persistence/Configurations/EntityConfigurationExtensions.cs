using ClaimsModule.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal static class EntityConfigurationExtensions
{
    public static PropertyBuilder<Guid> ConfigureSequentialGuidKey(
        this PropertyBuilder<Guid> property,
        string columnName)
    {
        property
            .HasColumnName(columnName)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWSEQUENTIALID()");
        return property;
    }

    public static void ConfigureAuditableColumns<T>(this EntityTypeBuilder<T> builder)
        where T : class, IAuditable
    {
        builder.Property(nameof(IAuditable.CreatedAt))
            .HasColumnType("datetimeoffset(7)")
            .IsRequired();
        builder.Property(nameof(IAuditable.UpdatedAt))
            .HasColumnType("datetimeoffset(7)");
        builder.Property(nameof(IAuditable.UserCreated));
        builder.Property(nameof(IAuditable.UserModified));
    }

    public static void ConfigureSoftDeleteColumns<T>(this EntityTypeBuilder<T> builder)
        where T : class, ISoftDeletable
    {
        builder.Property(nameof(ISoftDeletable.IsDeleted))
            .HasDefaultValue(false)
            .IsRequired();
        builder.Property(nameof(ISoftDeletable.DeletedAt))
            .HasColumnType("datetimeoffset(7)");
    }
}
