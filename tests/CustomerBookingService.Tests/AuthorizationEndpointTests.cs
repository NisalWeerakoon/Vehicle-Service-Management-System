using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using CustomerBookingService.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;
 
namespace CustomerBookingService.Tests;
 
/// <summary>
/// Covers everything that depends on [Authorize] / [Authorize(Roles=..)]:
/// GET /api/auth/me, POST /api/auth/logout, GET /api/auth/admin-test.
/// </summary>
public class AuthorizationEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
 
    public AuthorizationEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
 
    /// <summary>Mints a real JWT for a seeded user via the app's own ITokenService.</summary>
    private string CreateTokenFor(User user)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        return tokenService.CreateToken(user);
    }
 
    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task Me_WithValidToken_ReturnsUserClaims()
    {
        var user = await _factory.SeedUserAsync("me.endpoint@example.com", "Password123!", UserRole.Customer);
        var token = CreateTokenFor(user);
 
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
 
        var response = await _client.SendAsync(request);
 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
 
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body!["email"].Should().Be("me.endpoint@example.com");
        body["role"].Should().Be("Customer");
        body["userId"].Should().Be(user.Id.ToString());
    }
 
    [Fact]
    public async Task Me_WithTamperedToken_Returns401()
    {
        var user = await _factory.SeedUserAsync("tampered@example.com", "Password123!");
        var token = CreateTokenFor(user);
        var tampered = token[..^5] + "aaaaa"; // corrupt the signature
 
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tampered);
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task Logout_WithoutToken_Returns401()
    {
        var response = await _client.PostAsync("/api/auth/logout", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task Logout_WithValidToken_Returns200()
    {
        var user = await _factory.SeedUserAsync("logout@example.com", "Password123!");
        var token = CreateTokenFor(user);
 
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
 
    [Fact]
    public async Task AdminTest_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/admin-test");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task AdminTest_WithCustomerRole_Returns403()
    {
        var user = await _factory.SeedUserAsync("customer.role@example.com", "Password123!", UserRole.Customer);
        var token = CreateTokenFor(user);
 
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/admin-test");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
 
        var response = await _client.SendAsync(request);
 
        // Authenticated but wrong role -> 403, NOT 401.
        // A common bug is these two getting swapped.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
 
    [Theory]
    [InlineData(UserRole.ServiceAdvisor)]
    [InlineData(UserRole.Mechanic)]
    [InlineData(UserRole.InventoryOfficer)]
    [InlineData(UserRole.Accounts)]
    public async Task AdminTest_WithAnyNonAdminStaffRole_Returns403(UserRole role)
    {
        var user = await _factory.SeedUserAsync($"{role}@example.com", "Password123!", role);
        var token = CreateTokenFor(user);
 
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/admin-test");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
 
    [Fact]
    public async Task AdminTest_WithAdministratorRole_Returns200()
    {
        var user = await _factory.SeedUserAsync("admin@example.com", "Password123!", UserRole.Administrator);
        var token = CreateTokenFor(user);
 
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/admin-test");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Me_WithExpiredToken_Returns401()
    {
        var user = await _factory.SeedUserAsync("expired@example.com", "Password123!");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-signing-key-please-do-not-use-in-prod-32chars+"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var tokenOptions = new JwtSecurityToken(
            issuer: "VehicleServiceCenter",
            audience: "VehicleServiceCenterClients",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: creds
        );
        var expiredToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}