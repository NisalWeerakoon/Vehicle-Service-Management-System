using System.Net.Http.Json;
using CustomerBookingService.Data;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerBookingService.IntegrationTests.TestHelpers;

public static class AuthHelper
{
    /// <summary>
    /// Registers a new Customer account through the real /api/auth/register
    /// endpoint and returns the JWT. Mirrors exactly what the React app does.
    /// </summary>
    public static async Task<string> RegisterAndGetCustomerTokenAsync(
        HttpClient client,
        string email,
        string password = "Password123!")
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.Token;
    }

    /// <summary>
    /// Seeds a Customer profile + links it to the given user id directly in the DB.
    /// The BookingsController requires user.CustomerId to be set before a
    /// customer can create bookings.
    /// </summary>
    public static async Task LinkCustomerProfileAsync(
        CustomWebApplicationFactory factory,
        string email,
        string fullName = "Test Customer",
        string phone = "0771234567")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerBookingDbContext>();

        var user = db.Users.First(u => u.Email == email.ToLowerInvariant());

        var customer = new Customer
        {
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            Phone = phone
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        user.CustomerId = customer.Id;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a staff user (ServiceAdvisor/Administrator/etc.) directly in the DB
    /// since /api/auth/register only ever creates Customer accounts, and logs in
    /// through the real endpoint to get a genuine JWT.
    /// </summary>
    public static async Task<string> SeedStaffAndGetTokenAsync(
        CustomWebApplicationFactory factory,
        HttpClient client,
        string email,
        UserRole role,
        string password = "StaffPass123!")
    {
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CustomerBookingDbContext>();

            db.Users.Add(new User
            {
                Email = email.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsActive = true
            });

            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.Token;
    }

    public static async Task<int> AddVehicleForCustomerAsync(
        CustomWebApplicationFactory factory,
        string customerEmail,
        string registrationNumber = "ABC-1234")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerBookingDbContext>();

        var customer = db.Customers.First(c => c.Email == customerEmail.ToLowerInvariant());

        var vehicle = new Vehicle
        {
            CustomerId = customer.Id,
            RegistrationNumber = registrationNumber,
            Make = "Toyota",
            Model = "Aqua",
            Year = 2020,
            FuelType = "Hybrid"
        };

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        return vehicle.Id;
    }
}
