using Xunit;

namespace Aegis.Api.IntegrationTests
{
    public class SecurityHeadersTests
        : IClassFixture<AegisWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SecurityHeadersTests(
            AegisWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Api_Response_Contains_Security_Headers()
        {
            var response = await _client.GetAsync(
                "/api/projects"
            );

            Assert.True(
                response.Headers.Contains(
                    "X-Content-Type-Options"
                )
            );

            Assert.True(
                response.Headers.Contains(
                    "X-Frame-Options"
                )
            );

            Assert.True(
                response.Headers.Contains(
                    "Referrer-Policy"
                )
            );

            Assert.True(
                response.Headers.Contains(
                    "Permissions-Policy"
                )
            );

            Assert.True(
                response.Headers.Contains(
                    "Content-Security-Policy"
                )
            );

            Assert.Equal(
                "nosniff",
                response.Headers
                    .GetValues(
                        "X-Content-Type-Options"
                    )
                    .Single()
            );

            Assert.Equal(
                "DENY",
                response.Headers
                    .GetValues(
                        "X-Frame-Options"
                    )
                    .Single()
            );

            Assert.Equal(
                "no-referrer",
                response.Headers
                    .GetValues(
                        "Referrer-Policy"
                    )
                    .Single()
            );
        }
    }
} 