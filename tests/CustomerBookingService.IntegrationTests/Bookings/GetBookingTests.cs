using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerBookingService.DTOs;
using CustomerBookingService.IntegrationTests.TestHelpers;
using CustomerBookingService.Models;
using Xunit;

namespace CustomerBookingService.IntegrationTests.Bookings;

public class GetBookingTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetBookingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task Get_BookingsMe_ReturnsOnlyBookingsBelongingToCaller()
    {
        const string emailA = "customerA@test.com";
        const string emailB = "customerB@test.com";

        var tokenA = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, emailA);
        await AuthHelper.LinkCustomerProfileAsync(_factory, emailA);
        var vehicleA = await AuthHelper.AddVehicleForCustomerAsync(_factory, emailA, "AAA-111");

        var tokenB = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, emailB);
        await AuthHelper.LinkCustomerProfileAsync(_factory, emailB);
        var vehicleB = await AuthHelper.AddVehicleForCustomerAsync(_factory, emailB, "BBB-222");

        // Customer A creates a booking
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = vehicleA,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "A's booking"
        });

        // Customer B creates a booking
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = vehicleB,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "B's booking"
        });

        // Customer A should only see their own booking
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var response = await _client.GetAsync("/api/bookings/me");

        response.EnsureSuccessStatusCode();
        var bookings = await response.Content.ReadFromJsonAsync<List<BookingResponseDto>>();

        Assert.NotNull(bookings);
        Assert.Single(bookings!);
        Assert.Equal("A's booking", bookings![0].RequestedServiceOrProblem);
    }

    [Fact]
    public async Task Get_BookingsMeById_ForAnotherCustomersBooking_Returns404()
    {
        const string emailA = "customerC@test.com";
        const string emailB = "customerD@test.com";

        var tokenA = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, emailA);
        await AuthHelper.LinkCustomerProfileAsync(_factory, emailA);
        var vehicleA = await AuthHelper.AddVehicleForCustomerAsync(_factory, emailA, "CCC-333");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var createResponse = await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = vehicleA,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "A's private booking"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<BookingResponseDto>();

        var tokenB = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, emailB);
        await AuthHelper.LinkCustomerProfileAsync(_factory, emailB);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var response = await _client.GetAsync($"/api/bookings/me/{created!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_BookingsById_AsAdministrator_ReturnsAnyBooking()
    {
        const string customerEmail = "customerE@test.com";
        const string adminEmail = "admin1@test.com";

        var customerToken = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, customerEmail);
        await AuthHelper.LinkCustomerProfileAsync(_factory, customerEmail);
        var vehicleId = await AuthHelper.AddVehicleForCustomerAsync(_factory, customerEmail, "DDD-444");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var createResponse = await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = vehicleId,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Needs admin visibility"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<BookingResponseDto>();

        var adminToken = await AuthHelper.SeedStaffAndGetTokenAsync(
            _factory, _client, adminEmail, UserRole.Administrator);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await _client.GetAsync($"/api/bookings/{created!.Id}");

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
        Assert.Equal("Needs admin visibility", dto!.RequestedServiceOrProblem);
    }

    [Fact]
    public async Task Get_BookingsById_WithCustomerToken_Returns403()
    {
        const string customerEmail = "customerF@test.com";

        var customerToken = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, customerEmail);
        await AuthHelper.LinkCustomerProfileAsync(_factory, customerEmail);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

        // /api/bookings/{id} is restricted to ServiceAdvisor/Administrator only
        var response = await _client.GetAsync("/api/bookings/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
