using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Aegis.Api.IntegrationTests
{
    public class AuthRateLimitTests
        : IClassFixture<AegisWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthRateLimitTests(
            AegisWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_Returns_429_After_Rate_Limit_Is_Exceeded()
        {
            var request = new
            {
                email = "ratelimit-test@example.com",
                password = "WrongPassword123!"
            };

            HttpResponseMessage? response = null;

            for (var i = 0; i < 6; i++)
            {
                response = await _client.PostAsJsonAsync(
                    "/api/auth/login",
                    request
                );
            }

            Assert.NotNull(response);
            Assert.Equal(
                HttpStatusCode.TooManyRequests,
                response.StatusCode
            );
        }
    }
}  