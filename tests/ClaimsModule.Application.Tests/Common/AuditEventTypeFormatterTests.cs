using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Domain.Enums;
using FluentAssertions;

namespace ClaimsModule.Application.Tests.Common;

public sealed class AuditEventTypeFormatterTests
{
    [Theory]
    [InlineData(AuditEventType.ClaimCreated, "CLAIM_CREATED")]
    [InlineData(AuditEventType.GlPostingSimulated, "GL_POSTING_SIMULATED")]
    [InlineData(AuditEventType.SlaBreachDetected, "SLA_BREACH_DETECTED")]
    public void ToSpecificationString_uses_frs_names(AuditEventType eventType, string expected) =>
        AuditEventTypeFormatter.ToSpecificationString(eventType).Should().Be(expected);

    [Theory]
    [InlineData("CLAIM_CREATED", AuditEventType.ClaimCreated)]
    [InlineData("GL_POSTING_FAILED", AuditEventType.GlPostingFailed)]
    [InlineData("ClaimCreated", AuditEventType.ClaimCreated)]
    public void Parse_accepts_spec_and_legacy_pascal_case(string value, AuditEventType expected) =>
        AuditEventTypeFormatter.Parse(value).Should().Be(expected);
}
