using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Infrastructure.Jobs;
using ClaimsModule.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.API.Tests;

public sealed class BackgroundJobsIntegrationTests : IClassFixture<ClaimsApiWithBackgroundJobsFactory>
{
    private const string SlaBreachDescription = "Claim has not been updated in 48 hours.";

    private static readonly Guid SeedPolicyId = Guid.Parse("aaaaaaaa-0001-0000-0000-000000000001");

    private readonly ClaimsApiWithBackgroundJobsFactory _factory;
    private readonly HttpClient _client;

    public BackgroundJobsIntegrationTests(ClaimsApiWithBackgroundJobsFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.GlSimulator.ShouldFail = false;
        _factory.Clock.UtcNow = new DateTimeOffset(2026, 5, 30, 12, 0, 0, TimeSpan.Zero);
    }

    [Fact]
    public async Task AutoApproved_reserve_triggers_gl_posting_with_simulated_audit()
    {
        await AuthenticateAsHandlerAsync();
        var claimId = await CreateClaimAsync();

        var reservePayload = new
        {
            transactionType = ReserveTransactionType.Add.ToString(),
            component = ReserveComponentType.Indemnity.ToString(),
            amount = 5000m,
            changeReason = "Initial indemnity reserve for GL integration test"
        };

        using var reserveRequest = ReserveRequest(claimId, reservePayload, Guid.NewGuid().ToString());
        var reserveResponse = await _client.SendAsync(reserveRequest);
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var reserves = await GetReservesAsync(claimId);
        var txn = reserves.GetProperty("transactions")[0];
        txn.GetProperty("postingStatus").GetString().Should().Be("Posted");

        var audit = await GetAuditAsync(claimId);
        var glEvents = audit.EnumerateArray()
            .Where(e => e.GetProperty("eventType").GetString() == "GL_POSTING_SIMULATED")
            .ToList();
        glEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task PostGLReserveChangeJob_second_run_is_idempotent()
    {
        await AuthenticateAsHandlerAsync();
        var claimId = await CreateClaimAsync();

        using var reserveRequest = ReserveRequest(
            claimId,
            new
            {
                transactionType = ReserveTransactionType.Add.ToString(),
                component = ReserveComponentType.Indemnity.ToString(),
                amount = 5000m,
                changeReason = "Reserve for idempotent GL job test"
            },
            Guid.NewGuid().ToString());

        var reserveResponse = await _client.SendAsync(reserveRequest);
        reserveResponse.EnsureSuccessStatusCode();
        var historyId = (await reserveResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("reserveHistoryId").GetGuid();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var history = await db.ReserveHistory.SingleAsync(h => h.Id == historyId);
        var job = scope.ServiceProvider.GetRequiredService<PostGLReserveChangeJob>();

        await job.ExecuteAsync(history.Id, history.ClaimId, history.IdempotencyKey.Value, null);
        await job.ExecuteAsync(history.Id, history.ClaimId, history.IdempotencyKey.Value, null);

        var audit = await GetAuditAsync(claimId);
        audit.EnumerateArray()
            .Count(e => e.GetProperty("eventType").GetString() == "GL_POSTING_SIMULATED")
            .Should().Be(1);
    }

    [Fact]
    public async Task Gl_posting_failure_then_retry_sets_posted()
    {
        await AuthenticateAsHandlerAsync();
        var claimId = await CreateClaimAsync();

        using var reserveRequest = ReserveRequest(
            claimId,
            new
            {
                transactionType = ReserveTransactionType.Add.ToString(),
                component = ReserveComponentType.Indemnity.ToString(),
                amount = 5000m,
                changeReason = "Reserve for GL failure and retry test"
            },
            Guid.NewGuid().ToString());

        var reserveResponse = await _client.SendAsync(reserveRequest);
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var historyId = (await reserveResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("reserveHistoryId").GetGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
            var history = await db.ReserveHistory.SingleAsync(h => h.Id == historyId);
            history.ResetPostingForRetry();
            await db.SaveChangesAsync();

            _factory.GlSimulator.ShouldFail = true;
            var job = scope.ServiceProvider.GetRequiredService<PostGLReserveChangeJob>();
            var act = () => job.ExecuteAsync(
                history.Id, history.ClaimId, history.IdempotencyKey.Value, null);
            await act.Should().ThrowAsync<InvalidOperationException>();

            history.MarkPostingFailed();
            await db.SaveChangesAsync();
        }

        _factory.GlSimulator.ShouldFail = false;
        await AuthenticateAsSupervisorAsync();
        var retryResponse = await _client.PostAsync(
            $"/api/claims/{claimId}/reserves/transactions/{historyId}/retry-gl",
            null);
        retryResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var reserves = await GetReservesAsync(claimId);
        reserves.GetProperty("transactions")[0].GetProperty("postingStatus").GetString()
            .Should().Be("Posted");
    }

    [Fact]
    public async Task SlaMonitoringJob_writes_breach_and_dedupes_within_24_hours()
    {
        await AuthenticateAsHandlerAsync();

        var now = new DateTimeOffset(2026, 5, 30, 12, 0, 0, TimeSpan.Zero);
        _factory.Clock.UtcNow = now.AddHours(-50);
        var claimId = await CreateClaimAsync();
        _factory.Clock.UtcNow = now;

        using (var scope = _factory.Services.CreateScope())
        {
            var job = scope.ServiceProvider.GetRequiredService<SlaMonitoringJob>();
            await job.ExecuteAsync();
            await job.ExecuteAsync();
        }

        var audit = await GetAuditAsync(claimId);
        var breaches = audit.EnumerateArray()
            .Where(e => e.GetProperty("eventType").GetString() == "SLA_BREACH_DETECTED")
            .ToList();
        breaches.Should().HaveCount(1);
        breaches[0].GetProperty("description").GetString().Should().Be(SlaBreachDescription);

        _factory.Clock.UtcNow = _factory.Clock.UtcNow.AddHours(25);

        using (var scope = _factory.Services.CreateScope())
        {
            var job = scope.ServiceProvider.GetRequiredService<SlaMonitoringJob>();
            await job.ExecuteAsync();
        }

        audit = await GetAuditAsync(claimId);
        audit.EnumerateArray()
            .Count(e => e.GetProperty("eventType").GetString() == "SLA_BREACH_DETECTED")
            .Should().Be(2);
    }

    [Fact]
    public async Task GlPostingFailureApplier_marks_failed_and_writes_gl_posting_failed_audit()
    {
        await AuthenticateAsHandlerAsync();
        var claimId = await CreateClaimAsync();

        using var reserveRequest = ReserveRequest(
            claimId,
            new
            {
                transactionType = ReserveTransactionType.Add.ToString(),
                component = ReserveComponentType.Indemnity.ToString(),
                amount = 5000m,
                changeReason = "Reserve for GL failure applier test"
            },
            Guid.NewGuid().ToString());

        var reserveResponse = await _client.SendAsync(reserveRequest);
        reserveResponse.EnsureSuccessStatusCode();
        var historyId = (await reserveResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("reserveHistoryId").GetGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
            var history = await db.ReserveHistory.SingleAsync(h => h.Id == historyId);
            history.ResetPostingForRetry();
            await db.SaveChangesAsync();
        }

        GlPostingFailureApplier.Apply(
            _factory.Services.GetRequiredService<IServiceScopeFactory>(),
            historyId,
            "Simulated Hangfire failure after retries exhausted");

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var updated = await assertDb.ReserveHistory.SingleAsync(h => h.Id == historyId);
        updated.PostingStatus.Should().Be(ReservePostingStatus.Failed);

        (await assertDb.ClaimAuditLog.CountAsync(a =>
            a.ClaimId == claimId && a.EventType == AuditEventType.GlPostingFailed)).Should().Be(1);
    }

    private async Task AuthenticateAsHandlerAsync() => await AuthenticateAsync("handler");

    private async Task AuthenticateAsSupervisorAsync() => await AuthenticateAsync("supervisor");

    private async Task AuthenticateAsync(string role)
    {
        var tokenResponse = await _client.PostAsJsonAsync("/api/auth/token", new { role });
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
            lossDate = _factory.Clock.UtcNow.AddDays(-2),
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

    private async Task<JsonElement> GetReservesAsync(Guid claimId)
    {
        var response = await _client.GetAsync($"/api/claims/{claimId}/reserves");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>())!;
    }

    private async Task<JsonElement> GetAuditAsync(Guid claimId)
    {
        var response = await _client.GetAsync($"/api/claims/{claimId}/audit?pageNumber=1&pageSize=50");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("items");
    }

    private static HttpRequestMessage ReserveRequest(Guid claimId, object payload, string idempotencyKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/claims/{claimId}/reserves")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        return request;
    }
}
