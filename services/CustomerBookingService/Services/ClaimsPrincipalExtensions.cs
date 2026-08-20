using System.Security.Claims;
using CustomerBookingService.Models;

namespace CustomerBookingService.Services;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        return int.TryParse(value, out var id)
            ? id
            : null;
    }

    public static int? GetCustomerId(this ClaimsPrincipal user)
    {
        var value =
            user.FindFirstValue("customerId");

        return int.TryParse(value, out var id)
            ? id
            : null;
    }

    public static UserRole? GetRole(this ClaimsPrincipal user)
    {
        var value =
            user.FindFirstValue(
                ClaimTypes.Role
            );

        return Enum.TryParse<UserRole>(
            value,
            out var role
        )
            ? role
            : null;
    }

    public static bool IsStaff(this ClaimsPrincipal user)
    {
        var role = user.GetRole();

        return role.HasValue &&
               role.Value != UserRole.Customer;
    }
}