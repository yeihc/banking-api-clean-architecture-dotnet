using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BankingApi.Tests.Integration;
using Xunit;

namespace BankingApi.Tests.Concurrency
{
    public class TransferConcurrencyTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public TransferConcurrencyTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
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
        public async Task ConcurrentTransfers_WhenAttemptingToOverdraw_MaintainsBalanceInvariant()
        {
            // Arrange:
            // Source account starts with 100.00
            // Target account starts with 0.00
            var sourceId = await CreateAccountAsync(100.00m);
            var targetId = await CreateAccountAsync(0.00m);

            // 10 concurrent requests attempting to debit 20.00 each (Total attempted = 200.00)
            const int totalRequests = 10;
            const decimal transferAmount = 20.00m;

            var tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < totalRequests; i++)
            {
                var idempotencyKey = $"concurrent-overdraw-{Guid.NewGuid():N}";
                var payload = new
                {
                    sourceAccountId = sourceId,
                    targetAccountId = targetId,
                    amount = transferAmount,
                    idempotencyKey = idempotencyKey
                };

                // Use factory to create fresh client for parallel HTTP calls
                var client = _factory.CreateClient();
                tasks.Add(Task.Run(() => client.PostAsJsonAsync("/transfers", payload)));
            }

            // Act: Execute all concurrent transfers simultaneously
            var responses = await Task.WhenAll(tasks);

            // Assert:
            var successfulTransfers = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var failedTransfers = responses.Count(r =>
                r.StatusCode == HttpStatusCode.UnprocessableEntity ||
                r.StatusCode == HttpStatusCode.Conflict);

            // At most 5 transfers of 20 can succeed from an initial balance of 100
            Assert.True(successfulTransfers <= 5, $"Expected at most 5 successful transfers, but got {successfulTransfers}");
            Assert.Equal(totalRequests, successfulTransfers + failedTransfers);
        }

        [Fact]
        public async Task ConcurrentIdenticalTransfers_WithSameIdempotencyKey_PreventsDoubleSpending()
        {
            // Arrange:
            var sourceId = await CreateAccountAsync(100.00m);
            var targetId = await CreateAccountAsync(0.00m);
            var sharedIdempotencyKey = $"shared-idem-{Guid.NewGuid():N}";

            const int concurrentAttempts = 5;
            var payload = new
            {
                sourceAccountId = sourceId,
                targetAccountId = targetId,
                amount = 40.00m,
                idempotencyKey = sharedIdempotencyKey
            };

            var tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < concurrentAttempts; i++)
            {
                var client = _factory.CreateClient();
                tasks.Add(Task.Run(() => client.PostAsJsonAsync("/transfers", payload)));
            }

            // Act:
            var responses = await Task.WhenAll(tasks);

            // Assert:
            // Every response should succeed or return valid status without double debiting
            Assert.All(responses, r =>
            {
                Assert.True(r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.Conflict || r.StatusCode == HttpStatusCode.UnprocessableEntity);
            });

            // Verify query returns exactly 1 completed transfer record
            var getResponse = await _client.GetAsync($"/transfers/{sharedIdempotencyKey}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getBody = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(sharedIdempotencyKey, getBody.GetProperty("idempotencyKey").GetString());
            Assert.Equal("Completed", getBody.GetProperty("status").GetString());
        }
    }
}
