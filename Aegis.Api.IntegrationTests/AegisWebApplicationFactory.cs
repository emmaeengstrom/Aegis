using Aegis.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aegis.Api.IntegrationTests
{
    public class AegisWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.UseSetting(
                "Jwt:Key",
                "AegisIntegrationTestJwtKey_2026_AtLeast32CharactersLong");

            builder.UseSetting("Jwt:Issuer", "Aegis.Api");
            builder.UseSetting("Jwt:Audience", "Aegis.Client");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<AegisDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var password =
                    Environment.GetEnvironmentVariable("AEGIS_TEST_DB_PASSWORD");

                var testConnectionString =
                    $"Host=localhost;Port=5432;Database=aegis_test;Username=postgres;Password={password}";

                services.AddDbContext<AegisDbContext>(options =>
                    options.UseNpgsql(testConnectionString));

                var serviceProvider = services.BuildServiceProvider();

                using var scope = serviceProvider.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                db.Database.Migrate();
            });
        }
    }
}