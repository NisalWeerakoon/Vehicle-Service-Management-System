using CustomerBookingService.Data;
using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.UnitTests.TestHelpers;

/// <summary>
/// Creates a brand-new, isolated EF Core InMemory database for every test.
/// Using a fresh Guid as the database name means tests never leak state
/// into one another, even when xUnit runs them in parallel.
/// </summary>
public static class DbContextFactory
{
    public static CustomerBookingDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<CustomerBookingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerBookingDbContext(options);
    }
}
