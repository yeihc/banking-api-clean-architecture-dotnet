using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace BankingApi.Tests.Integration
{
    public class AccountEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AccountEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_OpenAccount_WithValidPayload_Returns201CreatedAndAccountDetails()
        {
            var customerId = Guid.NewGuid();
            var payload = new
            {
                customerId = customerId,
                initialDeposit = 500.00m
            };

            var response = await _client.PostAsJsonAsync("/accounts", payload);
            var content = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.Created, $"Actual status: {response.StatusCode}. Content: {content}");

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(body.TryGetProperty("accountId", out var accountId));
            Assert.NotEqual(Guid.Empty, accountId.GetGuid());

            Assert.True(body.TryGetProperty("accountNumber", out var accountNumber));
            Assert.StartsWith("ACC-", accountNumber.GetString());

            Assert.True(body.TryGetProperty("status", out var status));
            Assert.Equal("ACTIVE", status.GetString());
        }

        [Fact]
        public async Task Post_OpenAccount_WithNegativeDeposit_Returns400BadRequest()
        {
            var payload = new
            {
                customerId = Guid.NewGuid(),
                initialDeposit = -100.00m
            };

            var response = await _client.PostAsJsonAsync("/accounts", payload);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(body.TryGetProperty("message", out var message));
            Assert.Contains("Initial deposit cannot be negative", message.GetString());
        }
    }
}
