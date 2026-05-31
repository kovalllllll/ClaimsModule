using System.Text.Json;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Claims.EventHandlers;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClaimsModule.Application.Tests.Claims;

public sealed class ClaimStatusChangedEventHandlerTests
{
    [Fact]
    public async Task Handle_ClaimReopened_uses_reason_json_as_newValue()
    {
        var audit = new Mock<IAuditLogService>();
        string? capturedNewValue = null;

        audit.Setup(a => a.WriteAsync(
                It.IsAny<Guid>(),
                It.IsAny<AuditEventType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, AuditEventType, string, Guid?, string?, string?, Guid?, string?, CancellationToken>(
                (_, _, _, _, _, newValue, _, _, _) => capturedNewValue = newValue)
            .Returns(Task.CompletedTask);

        var handler = new ClaimStatusChangedEventHandler(audit.Object);
        var claimId = Guid.NewGuid();
        var evt = new ClaimStatusChangedEvent(
            claimId,
            ClaimStatus.Closed,
            ClaimStatus.Reopened,
            Reason: "Reopened for additional investigation");

        await handler.Handle(new DomainEventNotification<ClaimStatusChangedEvent>(evt), CancellationToken.None);

        var json = JsonDocument.Parse(capturedNewValue!);
        json.RootElement.GetProperty("reason").GetString()
            .Should().Be("Reopened for additional investigation");
    }

    [Fact]
    public async Task Handle_ClaimClosed_uses_closure_reason_json_as_newValue()
    {
        var audit = new Mock<IAuditLogService>();
        string? capturedNewValue = null;

        audit.Setup(a => a.WriteAsync(
                It.IsAny<Guid>(),
                It.IsAny<AuditEventType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, AuditEventType, string, Guid?, string?, string?, Guid?, string?, CancellationToken>(
                (_, _, _, _, _, newValue, _, _, _) => capturedNewValue = newValue)
            .Returns(Task.CompletedTask);

        var handler = new ClaimStatusChangedEventHandler(audit.Object);
        var evt = new ClaimStatusChangedEvent(
            Guid.NewGuid(),
            ClaimStatus.Open,
            ClaimStatus.Closed,
            Reason: "All conditions met");

        await handler.Handle(new DomainEventNotification<ClaimStatusChangedEvent>(evt), CancellationToken.None);

        var json = JsonDocument.Parse(capturedNewValue!);
        json.RootElement.GetProperty("reason").GetString().Should().Be("All conditions met");
    }
}
