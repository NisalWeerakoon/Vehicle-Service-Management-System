using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using CustomerBookingService.DTOs;
using FluentAssertions;
using Xunit;
 
namespace CustomerBookingService.Tests;
 
public class RegisterEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
 
    public RegisterEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
 
    [Fact]
    public async Task Register_WithValidData_Returns200AndToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "new.customer@example.com",
            Password = "Password123!"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
 
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Email.Should().Be("new.customer@example.com");
        body.Role.Should().Be("Customer");
        body.UserId.Should().BeGreaterThan(0);
    }
 
    [Fact]
    public async Task Register_NormalizesEmail_TrimsAndLowercases()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "  Mixed.Case@Example.com  ",
            Password = "Password123!"
        });
 
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.Email.Should().Be("mixed.case@example.com");
    }
 
    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        var email = "duplicate@example.com";
 
        var first = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = "Password123!"
        });
        first.StatusCode.Should().Be(HttpStatusCode.OK);
 
        var second = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = "AnotherPassword1!"
        });
 
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task Register_DuplicateEmail_IsCaseInsensitive()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "case.test@example.com",
            Password = "Password123!"
        });
 
        var second = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "CASE.TEST@EXAMPLE.COM",
            Password = "Password123!"
        });
 
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    [Theory]
    [InlineData("", "Password123!")]              // missing email
    [InlineData("not-an-email", "Password123!")]  // invalid email format
    [InlineData("valid@example.com", "short")]     // password under 8 chars
    [InlineData("valid@example.com", "")]           // missing password
    public async Task Register_WithInvalidInput_Returns400(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = password
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task Register_NewUser_DefaultsToCustomerRole()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "roletest@example.com",
            Password = "Password123!"
        });
 
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.Role.Should().Be("Customer");
    }
 
    [Fact]
    public async Task Register_ConcurrentSameEmail_OnlyOneSucceeds()
    {
        // Fires two registrations with the same email at (almost) the same
        // instant. Exactly one must succeed; the other must fail cleanly
        // with 400 -- not throw an unhandled 500 from a DB constraint.
        var email = "race.condition@example.com";
 
        var task1 = _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = "Password123!"
        });
        var task2 = _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = "Password123!"
        });
 
        var results = await Task.WhenAll(task1, task2);
 
        var successCount = results.Count(r => r.StatusCode == HttpStatusCode.OK);
        var badRequestCount = results.Count(r => r.StatusCode == HttpStatusCode.BadRequest);
 
        successCount.Should().Be(1, "only one of two identical concurrent registrations should succeed");
        badRequestCount.Should().Be(1);
 
        // Explicitly rule out a 500 from an unhandled DbUpdateException
        results.Should().NotContain(r => (int)r.StatusCode >= 500);
    }
 
    [Fact]
    public async Task Register_TokenExpiresAt_MatchesTokenExpClaim()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "expiry.check@example.com",
            Password = "Password123!"
        });
 
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(body!.Token);
 
        // ExpiresAt in the response body is computed separately from the
        // token's own "exp" claim -- they must agree, or clients relying on
        // ExpiresAt will be wrong about when the token actually dies.
        body.ExpiresAt.Should().BeCloseTo(jwt.ValidTo, TimeSpan.FromMinutes(1));
    }
 
    [Fact]
    public async Task Register_PasswordAtMaxLength_Succeeds()
    {
        var password = new string('a', 100); // exactly [MaxLength(100)]
 
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "maxpw@example.com",
            Password = password
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
 
    [Fact]
    public async Task Register_PasswordOverMaxLength_Returns400()
    {
        var password = new string('a', 101); // one over [MaxLength(100)]
 
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "overmaxpw@example.com",
            Password = password
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task Register_EmailAtMaxLength_Succeeds()
    {
        // [MaxLength(150)] -- build a valid-format email exactly 150 chars long
        var localPart = new string('a', 150 - "@example.com".Length);
        var email = $"{localPart}@example.com";
        email.Length.Should().Be(150);
 
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = "Password123!"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
 
    [Fact]
    public async Task Register_EmailOverMaxLength_Returns400()
    {
        var localPart = new string('a', 151 - "@example.com".Length);
        var email = $"{localPart}@example.com";
        email.Length.Should().Be(151);
 
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = email,
            Password = "Password123!"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task Register_WithMalformedJsonBody_Returns400()
    {
        var content = new StringContent("{ this is not valid json", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register", content);
 
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task Register_WeakPassword_CurrentlyAccepted_NoComplexityRule()
    {
        // CHARACTERIZATION TEST, not an endorsement: RegisterDto only enforces
        // [MinLength(8)], so "aaaaaaaa" currently passes. This test documents
        // that as current behavior. If a complexity rule gets added later,
        // this test should start failing -- that's the point: it'll force
        // someone to consciously update it instead of the gap staying silent.
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "weakpw@example.com",
            Password = "aaaaaaaa"
        });
 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
 
    [Fact]
    public async Task Register_WithEmptyBody_Returns400()
    {
        var content = new StringContent("", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register", content);
 
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}