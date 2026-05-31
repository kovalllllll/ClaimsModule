using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class AuditEventTypeValueConverter : ValueConverter<AuditEventType, string>
{
    public AuditEventTypeValueConverter()
        : base(
            eventType => AuditEventTypeFormatter.ToSpecificationString(eventType),
            value => AuditEventTypeFormatter.Parse(value))
    {
    }
}
