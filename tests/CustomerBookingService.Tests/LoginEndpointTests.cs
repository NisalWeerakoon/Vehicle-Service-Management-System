using System.Net;
using System.Net.Http.Json;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using FluentAssertions;
using Xunit;
 
namespace CustomerBookingService.Tests;
 
public class LoginEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
 
    public LoginEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
 
    [Fact]
    public async Task Login_WithCorrectCredentials_Returns200AndToken()
    {
        await _factory.SeedUserAsync("login.ok@example.com", "CorrectPass1!");
 
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "login.ok@example.com",
            Password = "CorrectPass1!"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
 
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
    }
 
    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        await _factory.SeedUserAsync("login.wrongpw@example.com", "CorrectPass1!");
 
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "login.wrongpw@example.com",
            Password = "WrongPassword!"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "does.not.exist@example.com",
            Password = "Whatever123!"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task Login_DoesNotLeak_WhetherEmailExists()
    {
        // Security regression guard: wrong-password and unknown-email
        // responses should be indistinguishable (same status + message),
        // so an attacker can't enumerate valid accounts.
        await _factory.SeedUserAsync("enum.guard@example.com", "CorrectPass1!");
 
        var wrongPassword = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "enum.guard@example.com",
            Password = "WrongPassword!"
        });
 
        var unknownEmail = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "totally.unknown@example.com",
            Password = "WrongPassword!"
        });
 
        wrongPassword.StatusCode.Should().Be(unknownEmail.StatusCode);
 
        var msg1 = await wrongPassword.Content.ReadAsStringAsync();
        var msg2 = await unknownEmail.Content.ReadAsStringAsync();
        msg1.Should().Be(msg2);
    }
 
    [Fact]
    public async Task Login_WithDeactivatedAccount_Returns401()
    {
        await _factory.SeedUserAsync(
            "deactivated@example.com",
            "CorrectPass1!",
            role: UserRole.Customer,
            isActive: false);
 
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "deactivated@example.com",
            Password = "CorrectPass1!"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task Login_IsCaseInsensitiveOnEmail()
    {
        await _factory.SeedUserAsync("case.login@example.com", "CorrectPass1!");
 
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "CASE.LOGIN@EXAMPLE.COM",
            Password = "CorrectPass1!"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}