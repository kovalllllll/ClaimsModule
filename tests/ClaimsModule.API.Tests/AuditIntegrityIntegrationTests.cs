using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Domain.Audit;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.API.Tests;

public sealed class AuditIntegrityIntegrationTests : IClassFixture<ClaimsApiWithBackgroundJobsFactory>
{
    private static readonly Guid SeedPolicyId = Guid.Parse("aaaaaaaa-0001-0000-0000-000000000001");

    private readonly ClaimsApiWithBackgroundJobsFactory _factory;
    private readonly HttpClient _client;

    public AuditIntegrityIntegrationTests(ClaimsApiWithBackgroundJobsFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateClaim_audit_uses_frs_event_types_and_shared_correlation_id()
    {
        var correlationId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId.ToString());

        await AuthenticateAsHandlerAsync();
        var claimId = await CreateClaimAsync();

        var audit = await GetAuditAsync(claimId);
        audit.GetArrayLength().Should().BeGreaterThan(0);

        audit.EnumerateArray()
            .Select(e => e.GetProperty("eventType").GetString())
            .Should()
            .Contain("CLAIM_CREATED");

        foreach (var entry in audit.EnumerateArray())
        {
            if (entry.TryGetProperty("correlationId", out var cid) && cid.ValueKind != JsonValueKind.Null)
            {
                cid.GetGuid().Should().Be(correlationId);
            }
        }
    }

    [Fact]
    public void ClaimAuditLog_modify_or_delete_is_blocked_by_append_only_interceptor()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        var entry = ClaimAuditLog.Create(
            claimId: Guid.NewGuid(),
            organisationId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            eventType: AuditEventType.ClaimCreated,
            description: "Append-only enforcement test",
            createdByUserId: null,
            createdAt: DateTimeOffset.UtcNow);

        db.ClaimAuditLog.Add(entry);
        db.SaveChanges();

        db.Entry(entry).Property(e => e.Description).CurrentValue = "mutated";
        db.Entry(entry).State = EntityState.Modified;

        var act = () => db.SaveChanges();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*append-only*");
    }

    [Fact]
    public async Task Status_transition_audit_uses_json_old_and_new_values()
    {
        await AuthenticateAsHandlerAsync();
        var claimId = await CreateClaimAsync();

        var detailResponse = await _client.GetAsync($"/api/claims/{claimId}");
        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>();
        var rowVer = detail.GetProperty("rowVer").GetString()!;

        var transitionRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/claims/{claimId}/status")
        {
            Content = JsonContent.Create(new { targetStatus = "Open" })
        };
        transitionRequest.Headers.TryAddWithoutValidation("If-Match", rowVer);

        var transitionResponse = await _client.SendAsync(transitionRequest);
        transitionResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var audit = await GetAuditAsync(claimId);
        var statusChange = audit.EnumerateArray()
            .FirstOrDefault(e => e.GetProperty("eventType").GetString() == "STATUS_CHANGED");

        statusChange.ValueKind.Should().NotBe(JsonValueKind.Undefined);

        var oldValue = JsonDocument.Parse(statusChange.GetProperty("oldValue").GetString()!).RootElement;
        var newValue = JsonDocument.Parse(statusChange.GetProperty("newValue").GetString()!).RootElement;
        oldValue.GetProperty("status").GetString().Should().Be("Draft");
        newValue.GetProperty("status").GetString().Should().Be("Open");
    }

    private async Task AuthenticateAsHandlerAsync()
    {
        var tokenResponse = await _client.PostAsJsonAsync("/api/auth/token", new { role = "handler" });
        tokenResponse.EnsureSuccessStatusCode();
        var tokenBody = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = tokenBody.GetProperty("token").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<Guid> CreateClaimAsync()
    {
        var payload = new
        {
            policyId = SeedPolicyId,
            policyNumber = "POL-2024-001001",
            clientName = "Meridian Transport LLC",
            severity = ClaimSeverity.Standard.ToString(),
            lossDate = DateTimeOffset.UtcNow.AddDays(-2),
            lossDescription = "Integration test loss description with sufficient length for validation.",
            causeOfLossCode = "COL-FIRE",
            estimatedLossAmount = 25_000m,
            parties = new[]
            {
                new
                {
                    partyType = "Person",
                    role = "Claimant",
                    firstName = "Test",
                    lastName = "Claimant",
                    isPrimary = true
                }
            },
            riskObjects = Array.Empty<object>()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/claims")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString());
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("claimId").GetGuid();
    }

    private async Task<JsonElement> GetAuditAsync(Guid claimId)
    {
        var response = await _client.GetAsync($"/api/claims/{claimId}/audit?pageNumber=1&pageSize=50");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("items");
    }
}
