using System.Security.Claims;
using CustomerBookingService.Controllers;
using CustomerBookingService.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerBookingService.UnitTests.TestHelpers;

/// <summary>
/// BookingsController reads the logged-in user's id from
/// ClaimTypes.NameIdentifier (see TokenService.CreateToken).
/// This helper fakes that claim so we can unit test the controller
/// exactly like it behaves for a real, authenticated request.
/// </summary>
public static class ControllerTestHelpers
{
    public static BookingsController CreateController(
        CustomerBookingDbContext dbContext,
        int userId,
        string role = "Customer")
    {
        var controller = new BookingsController(dbContext);

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        }, authenticationType: "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }
}
