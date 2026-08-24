using System.Net;
using System.Net.Http.Json;
using CustomerBookingService.DTOs;
using Xunit;

namespace CustomerBookingService.IntegrationTests;

public class VehiclesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VehiclesEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Registers a fresh customer account and returns a bearer token.
    // Each call uses a unique email so tests don't collide.
    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"qa-{Guid.NewGuid():N}@example.com";
        const string password = "P@ssword123";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = password
        });
        registerResponse.EnsureSuccessStatusCode();

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }

    [Fact]
    public async Task GetMyVehicles_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/vehicles/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyVehicles_NewlyRegisteredCustomer_NoProfileYet_ReturnsBadRequest()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // A brand-new user has no CustomerId yet (profile not created),
        // so the controller should reject with 400, not 500.
        var response = await _client.GetAsync("/api/vehicles/me");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CreateMyVehicle_InvalidBody_ReturnsValidationError()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // Missing required fields entirely -> model validation (DataAnnotations) should reject.
        var response = await _client.PostAsJsonAsync("/api/vehicles/me", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CreateMyVehicle_YearOutsideDtoRange_ReturnsValidationError()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // VehicleCreateDto has [Range(1900, 2100)] on Year - this checks the
        // model-binding validation layer, which unit tests calling the
        // controller directly cannot exercise.
        var response = await _client.PostAsJsonAsync("/api/vehicles/me", new
        {
            registrationNumber = "RNG-0001",
            make = "Toyota",
            model = "Corolla",
            year = 2200,
            fuelType = "Petrol"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task StaffOnlyEndpoint_CalledByCustomerRole_ReturnsForbidden()
    {
        var token = await RegisterAndLoginAsync(); // registers as Customer role
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // /api/vehicles/customer/{id} is [Authorize(Roles = "ServiceAdvisor,Administrator")]
        var response = await _client.GetAsync("/api/vehicles/customer/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }
}
