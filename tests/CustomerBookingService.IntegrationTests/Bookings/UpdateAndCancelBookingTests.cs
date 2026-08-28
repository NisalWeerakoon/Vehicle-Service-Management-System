using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerBookingService.DTOs;
using CustomerBookingService.IntegrationTests.TestHelpers;
using CustomerBookingService.Models;
using Xunit;

namespace CustomerBookingService.IntegrationTests.Bookings;

public class UpdateAndCancelBookingTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UpdateAndCancelBookingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    public void Dispose() => _client.Dispose();

    private async Task<(string token, int bookingId)> CreatePendingBookingAsync(string email)
    {
        var token = await AuthHelper.RegisterAndGetCustomerTokenAsync(_client, email);
        await AuthHelper.LinkCustomerProfileAsync(_factory, email);
        var vehicleId = await AuthHelper.AddVehicleForCustomerAsync(_factory, email);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/bookings/me", new BookingCreateDto
        {
            VehicleId = vehicleId,
            PreferredDate = DateTime.UtcNow.AddDays(2),
            RequestedServiceOrProblem = "Initial request"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<BookingResponseDto>();

        return (token, created!.Id);
    }

    [Fact]
    public async Task Put_BookingsMe_UpdatesPendingBooking_Returns200()
    {
        var (token, bookingId) = await CreatePendingBookingAsync("update1@test.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync($"/api/bookings/me/{bookingId}", new BookingUpdateDto
        {
            PreferredDate = DateTime.UtcNow.AddDays(5),
            RequestedServiceOrProblem = "Changed my mind - full service instead"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
        Assert.Equal("Changed my mind - full service instead", dto!.RequestedServiceOrProblem);
    }

    [Fact]
    public async Task Put_BookingsMe_WithPastDate_Returns400()
    {
        var (token, bookingId) = await CreatePendingBookingAsync("update2@test.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync($"/api/bookings/me/{bookingId}", new BookingUpdateDto
        {
            PreferredDate = DateTime.UtcNow.AddDays(-1),
            RequestedServiceOrProblem = "Backdated update"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Patch_BookingsMeCancel_SetsStatusToCancelled_Returns200()
    {
        var (token, bookingId) = await CreatePendingBookingAsync("cancel1@test.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PatchAsync($"/api/bookings/me/{bookingId}/cancel", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
        Assert.Equal("Cancelled", dto!.Status);
    }

    [Fact]
    public async Task Patch_BookingsMeCancel_Twice_SecondCallReturns400()
    {
        var (token, bookingId) = await CreatePendingBookingAsync("cancel2@test.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PatchAsync($"/api/bookings/me/{bookingId}/cancel", content: null);
        var secondResponse = await _client.PatchAsync($"/api/bookings/me/{bookingId}/cancel", content: null);

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Put_BookingsStaff_WithServiceAdvisorToken_UpdatesBooking()
    {
        var (_, bookingId) = await CreatePendingBookingAsync("staffupdate1@test.com");

        var advisorToken = await AuthHelper.SeedStaffAndGetTokenAsync(
            _factory, _client, "advisor3@test.com", UserRole.ServiceAdvisor);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", advisorToken);

        var response = await _client.PutAsJsonAsync($"/api/bookings/{bookingId}", new BookingUpdateDto
        {
            PreferredDate = DateTime.UtcNow.AddDays(4),
            RequestedServiceOrProblem = "Rescheduled by staff"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Patch_BookingsStaffCancel_WithCustomerToken_Returns403()
    {
        var (token, bookingId) = await CreatePendingBookingAsync("staffcancel1@test.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // /api/bookings/{id}/cancel is staff-only
        var response = await _client.PatchAsync($"/api/bookings/{bookingId}/cancel", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
