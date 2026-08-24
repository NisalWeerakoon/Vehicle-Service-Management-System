using CustomerBookingService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CustomerBookingService.IntegrationTests;

/// <summary>
/// Boots the real CustomerBookingService app in-memory (real Program.cs,
/// real middleware pipeline, real controllers/auth) but:
///  - replaces the MySQL DbContext with EF Core InMemory
///  - injects a JWT signing key so token validation works without secrets.json
/// This requires Program.cs to expose:  public partial class Program { }
/// (add that one line at the bottom of the real Program.cs - see README).
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public readonly string DatabaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=localhost;Database=placeholder;");
        builder.UseSetting("Jwt:Key", "test-signing-key-please-make-this-long-enough-1234567890");
        builder.UseSetting("Jwt:Issuer", "VehicleServiceCenter");
        builder.UseSetting("Jwt:Audience", "VehicleServiceCenterClients");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            // Values Program.cs requires at startup (Jwt:Key / Issuer / Audience,
            // ConnectionStrings:DefaultConnection). The real connection string is
            // never used because we replace the DbContext registration below,
            // but Program.cs throws if it's missing, so we provide a placeholder.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=placeholder;",
                ["Jwt:Key"] = "test-signing-key-please-make-this-long-enough-1234567890",
                ["Jwt:Issuer"] = "VehicleServiceCenter",
                ["Jwt:Audience"] = "VehicleServiceCenterClients"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove all existing DbContext options & configuration descriptors (e.g. MySQL provider)
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<CustomerBookingDbContext>) ||
                            d.ServiceType == typeof(DbContextOptions) ||
                            d.ServiceType == typeof(CustomerBookingDbContext) ||
                            d.ServiceType.Name.Contains("DbContextOptions"))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Replace it with an isolated in-memory database per test run.
            services.AddDbContext<CustomerBookingDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
            });
        });
    }
}
