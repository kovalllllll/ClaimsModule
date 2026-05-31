using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Domain.Enums;
using FluentAssertions;

namespace ClaimsModule.API.Tests;

public sealed class ClaimsApiSmokeTests : IClassFixture<ClaimsApiFactory>
{
  private static readonly Guid SeedPolicyId = Guid.Parse("aaaaaaaa-0001-0000-0000-000000000001");
  private static readonly Guid SeedOrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

  private readonly ClaimsApiFactory _factory;
  private readonly HttpClient _client;

  public ClaimsApiSmokeTests(ClaimsApiFactory factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Auth_token_returns_jwt()
  {
    var response = await _client.PostAsJsonAsync("/api/auth/token", new { role = "handler" });
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var body = await response.Content.ReadFromJsonAsync<JsonElement>();
    body.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task CreateClaim_with_idempotency_key_replays_same_claim()
  {
    await AuthenticateAsHandlerAsync();

    var idempotencyKey = Guid.NewGuid().ToString();
    var payload = BuildCreateClaimPayload();

    using var request1 = CreateClaimRequest(payload, idempotencyKey);
    var response1 = await _client.SendAsync(request1);
    var response1Body = await response1.Content.ReadAsStringAsync();
    response1.StatusCode.Should().Be(HttpStatusCode.Created, response1Body);
    var claim1 = JsonSerializer.Deserialize<JsonElement>(response1Body);
    var claimId1 = claim1.GetProperty("claimId").GetGuid();

    using var request2 = CreateClaimRequest(payload, idempotencyKey);
    var response2 = await _client.SendAsync(request2);
    response2.StatusCode.Should().Be(HttpStatusCode.Created);
    var claim2 = await response2.Content.ReadFromJsonAsync<JsonElement>();
    claim2.GetProperty("claimId").GetGuid().Should().Be(claimId1);
  }

  [Fact]
  public async Task OpenReserve_with_idempotency_key_replays_same_history()
  {
    await AuthenticateAsHandlerAsync();

    var claimId = await CreateClaimAsync();
    var idempotencyKey = Guid.NewGuid().ToString();
    var payload = new
    {
      transactionType = ReserveTransactionType.Add.ToString(),
      component = ReserveComponentType.Indemnity.ToString(),
      amount = 5000m,
      changeReason = "Initial indemnity reserve for integration test"
    };

    using var request1 = ReserveRequest(claimId, payload, idempotencyKey);
    var response1 = await _client.SendAsync(request1);
    response1.StatusCode.Should().Be(HttpStatusCode.Created);
    var body1 = await response1.Content.ReadFromJsonAsync<JsonElement>();
    var historyId1 = body1.GetProperty("reserveHistoryId").GetGuid();

    using var request2 = ReserveRequest(claimId, payload, idempotencyKey);
    var response2 = await _client.SendAsync(request2);
    response2.StatusCode.Should().Be(HttpStatusCode.Created);
    var body2 = await response2.Content.ReadFromJsonAsync<JsonElement>();
    body2.GetProperty("reserveHistoryId").GetGuid().Should().Be(historyId1);
  }

  [Fact]
  public async Task ApproveReserve_by_submitter_returns_422()
  {
    await AuthenticateAsHandlerAsync();

    var claimId = await CreateClaimAsync();
    var reserveKey = Guid.NewGuid().ToString();
    var reservePayload = new
    {
      transactionType = ReserveTransactionType.Add.ToString(),
      component = ReserveComponentType.Expense.ToString(),
      amount = 50_000m,
      changeReason = "Expense reserve pending supervisor approval path"
    };

    using var reserveRequest = ReserveRequest(claimId, reservePayload, reserveKey);
    var reserveResponse = await _client.SendAsync(reserveRequest);
    reserveResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    var reserveBody = await reserveResponse.Content.ReadFromJsonAsync<JsonElement>();
    var historyId = reserveBody.GetProperty("reserveHistoryId").GetGuid();

    var approveResponse = await _client.PostAsJsonAsync(
      $"/api/claims/{claimId}/reserves/{historyId}/approve",
      new { managerOverride = false });

    approveResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
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
    var payload = BuildCreateClaimPayload();
    using var request = CreateClaimRequest(payload, Guid.NewGuid().ToString());
    var response = await _client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadFromJsonAsync<JsonElement>();
    return body.GetProperty("claimId").GetGuid();
  }

  private static object BuildCreateClaimPayload() => new
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

  private static HttpRequestMessage CreateClaimRequest(object payload, string idempotencyKey)
  {
    var request = new HttpRequestMessage(HttpMethod.Post, "/api/claims")
    {
      Content = JsonContent.Create(payload)
    };
    request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
    return request;
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
