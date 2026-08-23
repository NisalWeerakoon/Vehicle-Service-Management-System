using CustomerBookingService.Data;
using CustomerBookingService.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
 
namespace CustomerBookingService.Tests;
 
/// <summary>
/// Boots the REAL Program.cs pipeline (real routing, real JWT auth, real
/// [Authorize]/[Authorize(Roles=..)] checks) but:
///   1. Replaces the MySQL DbContext with an in-memory SQLite connection
///      so tests don't need a real database or network access.
///   2. Injects known Jwt:Key / Jwt:Issuer / Jwt:Audience values so tests
///      can mint and validate tokens deterministically.
/// One instance = one isolated in-memory database (kept alive for the
/// lifetime of the connection so the schema survives between requests).
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("DataSource=:memory:");
 
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", "test-signing-key-please-do-not-use-in-prod-32chars+");
        builder.UseSetting("Jwt:Issuer", "VehicleServiceCenter");
        builder.UseSetting("Jwt:Audience", "VehicleServiceCenterClients");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=unused;Database=unused;");
 
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Deterministic JWT settings + a dummy MySQL connection string
            // (never actually used, but Program.cs throws at startup if
            // it's missing/blank).
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-signing-key-please-do-not-use-in-prod-32chars+",
                ["Jwt:Issuer"] = "VehicleServiceCenter",
                ["Jwt:Audience"] = "VehicleServiceCenterClients",
                ["ConnectionStrings:DefaultConnection"] = "Server=unused;Database=unused;"
            });
        });
 
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<CustomerBookingDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(CustomerBookingDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
 
            _connection.Open();
 
            services.AddDbContext<CustomerBookingDbContext>(options =>
                options.UseSqlite(_connection));
 
            // Build the schema on the in-memory DB
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CustomerBookingDbContext>();
            db.Database.EnsureCreated();
        });
    }
 
    /// <summary>Seeds a user directly (bypassing the API) for login/me/role tests.</summary>
    public async Task<User> SeedUserAsync(
        string email,
        string plainTextPassword,
        UserRole role = UserRole.Customer,
        bool isActive = true)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerBookingDbContext>();
 
        var user = new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword),
            Role = role,
            IsActive = isActive
        };
 
        db.Users.Add(user);
        await db.SaveChangesAsync();
 
        return user;
    }
 
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}