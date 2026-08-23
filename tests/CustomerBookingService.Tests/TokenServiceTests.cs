using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CustomerBookingService.Models;
using CustomerBookingService.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
 
namespace CustomerBookingService.Tests;
 
/// <summary>
/// Pure unit tests: no web host, no database. Just proves TokenService
/// builds a correct, well-formed JWT for a given User.
/// </summary>
public class TokenServiceTests
{
    private static ITokenService BuildTokenService(string? jwtKey = "unit-test-signing-key-32-characters-min")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = jwtKey,
                ["Jwt:Issuer"] = "VehicleServiceCenter",
                ["Jwt:Audience"] = "VehicleServiceCenterClients"
            })
            .Build();
 
        return new TokenService(config);
    }
 
    [Fact]
    public void CreateToken_IncludesExpectedClaims()
    {
        // Arrange
        var service = BuildTokenService();
        var user = new User
        {
            Id = 42,
            Email = "driver@example.com",
            Role = UserRole.Customer,
            PasswordHash = "irrelevant-for-this-test"
        };
 
        // Act
        var jwt = service.CreateToken(user);
 
        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
 
        token.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == "42");
 
        token.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email && c.Value == "driver@example.com");
 
        token.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Role && c.Value == "Customer");
 
        token.Issuer.Should().Be("VehicleServiceCenter");
        token.Audiences.Should().Contain("VehicleServiceCenterClients");
    }
 
    [Fact]
    public void CreateToken_IncludesCustomerIdClaim_WhenPresent()
    {
        var service = BuildTokenService();
        var user = new User
        {
            Id = 7,
            Email = "customer@example.com",
            Role = UserRole.Customer,
            CustomerId = 999,
            PasswordHash = "irrelevant-for-this-test"
        };
 
        var jwt = service.CreateToken(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
 
        token.Claims.Should().Contain(c => c.Type == "customerId" && c.Value == "999");
    }
 
    [Fact]
    public void CreateToken_OmitsCustomerIdClaim_WhenNull()
    {
        var service = BuildTokenService();
        var user = new User
        {
            Id = 8,
            Email = "staff@example.com",
            Role = UserRole.Administrator,
            CustomerId = null,
            PasswordHash = "irrelevant-for-this-test"
        };
 
        var jwt = service.CreateToken(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
 
        token.Claims.Should().NotContain(c => c.Type == "customerId");
    }
 
    [Fact]
    public void CreateToken_SetsExpiryAroundEightHours()
    {
        var service = BuildTokenService();
        var user = new User { Id = 1, Email = "a@b.com", Role = UserRole.Customer, PasswordHash = "x" };
 
        var jwt = service.CreateToken(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
 
        var expectedExpiry = DateTime.UtcNow.AddHours(8);
        token.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }
 
    [Fact]
    public void CreateToken_Throws_WhenJwtKeyMissing()
    {
        var service = BuildTokenService(jwtKey: null);
        var user = new User { Id = 1, Email = "a@b.com", Role = UserRole.Customer, PasswordHash = "x" };
 
        var act = () => service.CreateToken(user);
 
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT secret key*");
    }
}