using CustomerBookingService.Data;
using CustomerBookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerBookingService.Tests;
 
/// <summary>
/// Extra seeding helpers for the customer-profile feature. Kept in a
/// separate partial-style extension file so CustomWebApplicationFactory.cs
/// from the auth feature doesn't need to be touched/re-copied.
/// </summary>
public static class CustomerSeedingExtensions
{
    /// <summary>
    /// Seeds a User with the Customer role AND a linked Customer profile
    /// in one call (covers "already has a profile" scenarios).
    /// </summary>
    public static async Task<(User user, Customer customer)> SeedUserWithProfileAsync(
        this CustomWebApplicationFactory factory,
        string email,
        string plainTextPassword,
        string fullName = "Test Customer",
        string phone = "0771234567")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerBookingDbContext>();
 
        var normalizedEmail = email.ToLowerInvariant();
 
        var customer = new Customer
        {
            FullName = fullName,
            Email = normalizedEmail,
            Phone = phone
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
 
        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword),
            Role = UserRole.Customer,
            CustomerId = customer.Id
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
 
        return (user, customer);
    }
}