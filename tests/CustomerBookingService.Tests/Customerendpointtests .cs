using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using CustomerBookingService.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
 
namespace CustomerBookingService.Tests;
 
public class CustomerEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
 
    public CustomerEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
 
    private string TokenFor(User user)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        return tokenService.CreateToken(user);
    }
 
    private HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
 
    // ================= GET /api/customers/me =================
 
    [Fact]
    public async Task GetMyProfile_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/customers/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task GetMyProfile_AsStaffRole_Returns403()
    {
        var staff = await _factory.SeedUserAsync("staff.getme@example.com", "Password123!", UserRole.ServiceAdvisor);
        var token = TokenFor(staff);
 
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/customers/me", token));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
 
    [Fact]
    public async Task GetMyProfile_CustomerWithNoProfileYet_Returns404()
    {
        // A Customer-role user can exist (registered) before ever creating
        // a profile -- SeedUserAsync makes exactly that case.
        var user = await _factory.SeedUserAsync("noprofile@example.com", "Password123!", UserRole.Customer);
        var token = TokenFor(user);
 
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/customers/me", token));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
 
    [Fact]
    public async Task GetMyProfile_WithExistingProfile_Returns200AndCorrectData()
    {
        var (user, customer) = await _factory.SeedUserWithProfileAsync(
            "hasprofile@example.com", "Password123!", fullName: "Jane Driver", phone: "0712345678");
        var token = TokenFor(user);
 
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/customers/me", token));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
 
        var body = await response.Content.ReadFromJsonAsync<CustomerResponseDto>();
        body!.Id.Should().Be(customer.Id);
        body.FullName.Should().Be("Jane Driver");
        body.Phone.Should().Be("0712345678");
        body.Email.Should().Be("hasprofile@example.com");
    }
 
    // ================= POST /api/customers/me =================
 
    [Fact]
    public async Task CreateMyProfile_HappyPath_Returns201()
    {
        var user = await _factory.SeedUserAsync("create.ok@example.com", "Password123!", UserRole.Customer);
        var token = TokenFor(user);
 
        var request = Authorized(HttpMethod.Post, "/api/customers/me", token);
        request.Content = JsonContent.Create(new CustomerCreateDto
        {
            FullName = "New Customer",
            Email = "create.ok@example.com", // must match the account email
            Phone = "0770000000",
            Address = "123 Main St"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
 
        var body = await response.Content.ReadFromJsonAsync<CustomerResponseDto>();
        body!.FullName.Should().Be("New Customer");
    }
 
    [Fact]
    public async Task CreateMyProfile_EmailMismatchWithAccount_Returns400()
    {
        var user = await _factory.SeedUserAsync("account.email@example.com", "Password123!", UserRole.Customer);
        var token = TokenFor(user);
 
        var request = Authorized(HttpMethod.Post, "/api/customers/me", token);
        request.Content = JsonContent.Create(new CustomerCreateDto
        {
            FullName = "Mismatch Test",
            Email = "totally.different@example.com", // does NOT match account email
            Phone = "0770000000"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task CreateMyProfile_WhenProfileAlreadyExists_Returns400()
    {
        var (user, _) = await _factory.SeedUserWithProfileAsync("already.has@example.com", "Password123!");
        var token = TokenFor(user);
 
        var request = Authorized(HttpMethod.Post, "/api/customers/me", token);
        request.Content = JsonContent.Create(new CustomerCreateDto
        {
            FullName = "Duplicate Attempt",
            Email = "already.has@example.com",
            Phone = "0770000000"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task CreateMyProfile_AsStaffRole_Returns403()
    {
        // /me is Customer-only, even for staff creating their own account
        var staff = await _factory.SeedUserAsync("staff.createme@example.com", "Password123!", UserRole.Administrator);
        var token = TokenFor(staff);
 
        var request = Authorized(HttpMethod.Post, "/api/customers/me", token);
        request.Content = JsonContent.Create(new CustomerCreateDto
        {
            FullName = "Staff Trying",
            Email = "staff.createme@example.com",
            Phone = "0770000000"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
 
    [Theory]
    [InlineData("", "0770000000")]              // missing full name
    [InlineData("Valid Name", "")]               // missing phone
    public async Task CreateMyProfile_WithInvalidInput_Returns400(string fullName, string phone)
    {
        var testEmail = $"invalidbody_{Guid.NewGuid()}@example.com";
        var user = await _factory.SeedUserAsync(testEmail, "Password123!", UserRole.Customer);
        var token = TokenFor(user);
 
        var request = Authorized(HttpMethod.Post, "/api/customers/me", token);
        request.Content = JsonContent.Create(new CustomerCreateDto
        {
            FullName = fullName,
            Email = testEmail,
            Phone = phone
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    // ================= PUT /api/customers/me =================
 
    [Fact]
    public async Task UpdateMyProfile_HappyPath_Returns200AndUpdatesFields()
    {
        var (user, _) = await _factory.SeedUserWithProfileAsync(
            "update.ok@example.com", "Password123!", fullName: "Old Name", phone: "0700000000");
        var token = TokenFor(user);
 
        var request = Authorized(HttpMethod.Put, "/api/customers/me", token);
        request.Content = JsonContent.Create(new CustomerUpdateDto
        {
            FullName = "Updated Name",
            Phone = "0799999999",
            Address = "456 New Rd"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
 
        var body = await response.Content.ReadFromJsonAsync<CustomerResponseDto>();
        body!.FullName.Should().Be("Updated Name");
        body.Phone.Should().Be("0799999999");
        body.Address.Should().Be("456 New Rd");
        body.UpdatedAt.Should().NotBeNull();
    }
 
    [Fact]
    public async Task UpdateMyProfile_WhenNoProfileExists_Returns404()
    {
        var user = await _factory.SeedUserAsync("noprofile.update@example.com", "Password123!", UserRole.Customer);
        var token = TokenFor(user);
 
        var request = Authorized(HttpMethod.Put, "/api/customers/me", token);
        request.Content = JsonContent.Create(new CustomerUpdateDto
        {
            FullName = "Doesn't Matter",
            Phone = "0700000000"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
 
    [Fact]
    public async Task UpdateMyProfile_ClearingAddress_SetsAddressNull()
    {
        var (user, _) = await _factory.SeedUserWithProfileAsync(
            "clearaddr@example.com", "Password123!", fullName: "Has Address");
        var token = TokenFor(user);
 
        var request = Authorized(HttpMethod.Put, "/api/customers/me", token);
        request.Content = JsonContent.Create(new CustomerUpdateDto
        {
            FullName = "Has Address",
            Phone = "0700000000",
            Address = null
        });
 
        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponseDto>();
 
        body!.Address.Should().BeNull();
    }
 
    // ================= POST /api/customers (staff) =================
 
    [Fact]
    public async Task RegisterCustomerByStaff_AsServiceAdvisor_Returns201()
    {
        var staff = await _factory.SeedUserAsync("advisor.reg@example.com", "Password123!", UserRole.ServiceAdvisor);
        var token = TokenFor(staff);
 
        var request = Authorized(HttpMethod.Post, "/api/customers", token);
        request.Content = JsonContent.Create(new CustomerCreateDto
        {
            FullName = "Walk-in Customer",
            Email = "walkin@example.com",
            Phone = "0711111111"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
 
    [Fact]
    public async Task RegisterCustomerByStaff_AsCustomerRole_Returns403()
    {
        var customer = await _factory.SeedUserAsync("customer.tryreg@example.com", "Password123!", UserRole.Customer);
        var token = TokenFor(customer);
 
        var request = Authorized(HttpMethod.Post, "/api/customers", token);
        request.Content = JsonContent.Create(new CustomerCreateDto
        {
            FullName = "Should Not Work",
            Email = "shouldnotwork@example.com",
            Phone = "0711111111"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
 
    [Theory]
    [InlineData(UserRole.Mechanic)]
    [InlineData(UserRole.InventoryOfficer)]
    [InlineData(UserRole.Accounts)]
    public async Task RegisterCustomerByStaff_AsOtherStaffRoles_Returns403(UserRole role)
    {
        var staff = await _factory.SeedUserAsync($"{role}.reg@example.com", "Password123!", role);
        var token = TokenFor(staff);
 
        var request = Authorized(HttpMethod.Post, "/api/customers", token);
        request.Content = JsonContent.Create(new CustomerCreateDto
        {
            FullName = "Should Not Work",
            Email = $"{role}.blocked@example.com",
            Phone = "0711111111"
        });
 
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
 
    [Fact]
    public async Task RegisterCustomerByStaff_DuplicateEmail_Returns400()
    {
        var staff = await _factory.SeedUserAsync("advisor.dup@example.com", "Password123!", UserRole.ServiceAdvisor);
        var token = TokenFor(staff);
 
        var dto = new CustomerCreateDto
        {
            FullName = "First",
            Email = "dup.customer@example.com",
            Phone = "0711111111"
        };
 
        var first = Authorized(HttpMethod.Post, "/api/customers", token);
        first.Content = JsonContent.Create(dto);
        await _client.SendAsync(first);
 
        var second = Authorized(HttpMethod.Post, "/api/customers", token);
        second.Content = JsonContent.Create(dto);
        var response = await _client.SendAsync(second);
 
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
 
    // ================= GET /api/customers/{id} (staff) =================
 
    [Fact]
    public async Task GetCustomerById_AsAdministrator_Returns200()
    {
        var staff = await _factory.SeedUserAsync("admin.getbyid@example.com", "Password123!", UserRole.Administrator);
        var (_, customer) = await _factory.SeedUserWithProfileAsync("target.customer@example.com", "Password123!");
        var token = TokenFor(staff);
 
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/customers/{customer.Id}", token));
 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
 
    [Fact]
    public async Task GetCustomerById_NotFound_Returns404()
    {
        var staff = await _factory.SeedUserAsync("admin.notfound@example.com", "Password123!", UserRole.Administrator);
        var token = TokenFor(staff);
 
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/customers/999999", token));
 
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
 
    [Fact]
    public async Task GetCustomerById_AsCustomerRole_Returns403()
    {
        var (user, customer) = await _factory.SeedUserWithProfileAsync("self.lookup@example.com", "Password123!");
        var token = TokenFor(user);
 
        // Even looking up their OWN id via the staff route should be blocked --
        // customers must use /me, not /{id}.
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/customers/{customer.Id}", token));
 
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}