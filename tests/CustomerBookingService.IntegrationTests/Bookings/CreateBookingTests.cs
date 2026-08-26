using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerBookingService.Data;
using CustomerBookingService.DTOs;
using CustomerBookingService.IntegrationTests.TestHelpers;
using CustomerBookingService.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CustomerBookingService.IntegrationTests.Bookings;

public class CreateBookingTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateBookingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task Post_BookingsMe_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = 1,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Oil change"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_BookingsMe_WithValidCustomerToken_Returns201_AndPersistsPendingBooking()
    {
        const string email = "customer1@test.com";

        var token = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, email);
        await AuthHelper.LinkCustomerProfileAsync(_factory, email);
        var vehicleId = await AuthHelper.AddVehicleForCustomerAsync(_factory, email);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = vehicleId,
            PreferredDate = DateTime.UtcNow.AddDays(3),
            RequestedServiceOrProblem = "Annual service"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("Pending", body!.Status);
        Assert.StartsWith("BKG-", body.BookingReference);
    }

    [Fact]
    public async Task Post_BookingsMe_WithPastDate_Returns400()
    {
        const string email = "customer2@test.com";

        var token = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, email);
        await AuthHelper.LinkCustomerProfileAsync(_factory, email);
        var vehicleId = await AuthHelper.AddVehicleForCustomerAsync(_factory, email);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = vehicleId,
            PreferredDate = DateTime.UtcNow.AddDays(-2),
            RequestedServiceOrProblem = "Oil change"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_BookingsMe_ForSomeoneElsesVehicle_Returns400()
    {
        const string ownerEmail = "owner@test.com";
        const string attackerEmail = "attacker@test.com";

        // Vehicle belongs to "owner"
        var ownerToken = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, ownerEmail);
        await AuthHelper.LinkCustomerProfileAsync(_factory, ownerEmail);
        var vehicleId = await AuthHelper.AddVehicleForCustomerAsync(_factory, ownerEmail);

        // "attacker" tries to book service for a vehicle they don't own
        var attackerToken = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, attackerEmail);
        await AuthHelper.LinkCustomerProfileAsync(_factory, attackerEmail);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", attackerToken);

        var response = await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = vehicleId,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Free service please"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_BookingsStaff_WithCustomerRoleToken_Returns403()
    {
        const string email = "customer3@test.com";

        var token = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, email);
        await AuthHelper.LinkCustomerProfileAsync(_factory, email);
        var vehicleId = await AuthHelper.AddVehicleForCustomerAsync(_factory, email);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/bookings/staff", new StaffBookingCreateDto
        {
            CustomerId = 1,
            VehicleId = vehicleId,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Should not be allowed"
        });

        // A Customer JWT does not satisfy [Authorize(Roles = "ServiceAdvisor,Administrator")]
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_BookingsStaff_WithServiceAdvisorToken_Returns201()
    {
        const string customerEmail = "customer4@test.com";
        const string advisorEmail = "advisor1@test.com";

        await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, customerEmail);
        await AuthHelper.LinkCustomerProfileAsync(_factory, customerEmail);
        var vehicleId = await AuthHelper.AddVehicleForCustomerAsync(_factory, customerEmail);

        var advisorToken = await AuthHelper.SeedStaffAndGetTokenAsync(
            _factory, _client, advisorEmail, UserRole.ServiceAdvisor);

        int customerId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CustomerBookingDbContext>();
            customerId = db.Customers.First(c => c.Email == customerEmail).Id;
        }

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", advisorToken);

        var response = await _client.PostAsJsonAsync("/api/bookings/staff", new StaffBookingCreateDto
        {
            CustomerId = customerId,
            VehicleId = vehicleId,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Walk-in check engine light"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
