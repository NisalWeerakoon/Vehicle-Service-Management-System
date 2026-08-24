using System.Security.Claims;
using CustomerBookingService.Controllers;
using CustomerBookingService.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.UnitTests.TestHelpers;

/// <summary>
/// Shared helpers so every test file doesn't repeat the same
/// "build an in-memory DB + fake logged-in user" boilerplate.
/// </summary>
public static class ControllerTestHelpers
{
    // A fresh, isolated in-memory database per test (unique DB name per call).
    public static CustomerBookingDbContext BuildInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CustomerBookingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerBookingDbContext(options);
    }

    // Creates a VehiclesController with ControllerContext.User populated,
    // simulating a logged-in user with the given Id (matches
    // ClaimTypes.NameIdentifier used by the real controller).
    public static VehiclesController CreateVehiclesController(
        CustomerBookingDbContext dbContext,
        int userId,
        string role = "Customer")
    {
        var controller = new VehiclesController(dbContext);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }
}
