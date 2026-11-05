using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace BankingApi.Tests.Integration
{
    public class TransferEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TransferEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<Guid> CreateAccountAsync(decimal initialDeposit)
        {
            var response = await _client.PostAsJsonAsync("/accounts", new
            {
                customerId = Guid.NewGuid(),
                initialDeposit = initialDeposit
            });

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            return body.GetProperty("accountId").GetGuid();
        }

        [Fact]
        public async Task Post_Transfer_ValidRequest_CompletesTransferSuccessfully()
        {
            var sourceId = await CreateAccountAsync(300.00m);
            var targetId = await CreateAccountAsync(50.00m);
            var idempotencyKey = $"test-key-{Guid.NewGuid():N}";

            var transferPayload = new
            {
                sourceAccountId = sourceId,
                targetAccountId = targetId,
                amount = 100.00m,
                idempotencyKey = idempotencyKey
            };

            var response = await _client.PostAsJsonAsync("/transfers", transferPayload);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(idempotencyKey, body.GetProperty("idempotencyKey").GetString());
            Assert.Equal("ACCEPTED", body.GetProperty("status").GetString());
        }

        [Fact]
        public async Task Post_Transfer_WithSameIdempotencyKey_IsIdempotent()
        {
            var sourceId = await CreateAccountAsync(200.00m);
            var targetId = await CreateAccountAsync(0.00m);
            var idempotencyKey = $"idempotent-{Guid.NewGuid():N}";

            var payload = new
            {
                sourceAccountId = sourceId,
                targetAccountId = targetId,
                amount = 50.00m,
                idempotencyKey = idempotencyKey
            };

            // First execution
            var response1 = await _client.PostAsJsonAsync("/transfers", payload);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            // Second execution with same idempotency key
            var response2 = await _client.PostAsJsonAsync("/transfers", payload);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            var body2 = await response2.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(idempotencyKey, body2.GetProperty("idempotencyKey").GetString());
            Assert.Equal("ACCEPTED", body2.GetProperty("status").GetString());
        }

        [Fact]
        public async Task Post_Transfer_WithInsufficientFunds_Returns422UnprocessableEntity()
        {
            var sourceId = await CreateAccountAsync(20.00m);
            var targetId = await CreateAccountAsync(50.00m);
            var idempotencyKey = $"insufficient-{Guid.NewGuid():N}";

            var payload = new
            {
                sourceAccountId = sourceId,
                targetAccountId = targetId,
                amount = 100.00m,
                idempotencyKey = idempotencyKey
            };

            var response = await _client.PostAsJsonAsync("/transfers", payload);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("INSUFFICIENT_FUNDS", body.GetProperty("code").GetString());
        }

        [Fact]
        public async Task Get_TransferByIdempotencyKey_ReturnsTransferDetailsOrNotFound()
        {
            var sourceId = await CreateAccountAsync(100.00m);
            var targetId = await CreateAccountAsync(10.00m);
            var idempotencyKey = $"query-key-{Guid.NewGuid():N}";

            // 1. Initially non-existent -> 404
            var notFoundResponse = await _client.GetAsync($"/transfers/{idempotencyKey}");
            Assert.Equal(HttpStatusCode.NotFound, notFoundResponse.StatusCode);

            // 2. Perform transfer
            await _client.PostAsJsonAsync("/transfers", new
            {
                sourceAccountId = sourceId,
                targetAccountId = targetId,
                amount = 40.00m,
                idempotencyKey = idempotencyKey
            });

            // 3. Query transfer -> 200 OK
            var foundResponse = await _client.GetAsync($"/transfers/{idempotencyKey}");
            Assert.Equal(HttpStatusCode.OK, foundResponse.StatusCode);

            var body = await foundResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(idempotencyKey, body.GetProperty("idempotencyKey").GetString());
            Assert.Equal("Completed", body.GetProperty("status").GetString());
        }
    }
}
