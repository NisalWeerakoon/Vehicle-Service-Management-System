using CustomerBookingService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CustomerBookingService.IntegrationTests;

/// <summary>
/// Boots the real CustomerBookingService pipeline (routing, [Authorize],
/// JWT validation, model binding) in-memory for integration tests.
///
/// Two things Program.cs needs that we don't want to depend on in CI:
///   1. A real MySQL server           -> replaced with EF Core InMemory.
///   2. Jwt:Key (normally in user-secrets) -> supplied via env var below,
///      set BEFORE the host builds so WebApplicationBuilder's configuration
///      (which reads environment variables automatically) picks it up.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestJwtKey = "integration-test-super-secret-key-please-32chars-min";
    public const string TestJwtIssuer = "VehicleServiceCenter";
    public const string TestJwtAudience = "VehicleServiceCenterClients";

    public const string InMemoryDbName = "CustomerBookingIntegrationTestsDb";

    public CustomWebApplicationFactory()
    {
        // Must be set before the base class builds the host.
        Environment.SetEnvironmentVariable("Jwt__Key", TestJwtKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", TestJwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", TestJwtAudience);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove the MySQL DbContextOptions registered in Program.cs
            services.RemoveAll<DbContextOptions<CustomerBookingDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<CustomerBookingDbContext>>();

            services.AddDbContext<CustomerBookingDbContext>(options =>
                options.UseInMemoryDatabase(InMemoryDbName));
        });
    }

    /// <summary>
    /// Wipes and re-creates the shared in-memory database.
    /// Call from test constructors/fixture setup for a clean slate per test class.
    /// </summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerBookingDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}
