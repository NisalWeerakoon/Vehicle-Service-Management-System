using System.Security.Claims;
using JobMaintenanceService.Controllers;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Tests;

public class ServiceHistoryControllerTests
{
    [Fact]
    public async Task CustomerMustOwnVehicle_AndQueryUsesCustomerClaim()
    {
        var query = new RecordingQuery();
        var ownership = new StubOwnership(false);
        var controller = CreateController(query, ownership, "Customer", "20");

        var denied = await controller.GetByVehicle(10, CancellationToken.None);
        Assert.IsType<NotFoundObjectResult>(denied.Result);
        Assert.Equal(0, query.CallCount);

        ownership.IsOwned = true;
        var allowed = await controller.GetByVehicle(10, CancellationToken.None);
        Assert.IsType<OkObjectResult>(allowed.Result);
        Assert.Equal(20, query.CustomerId);
        Assert.Equal(10, query.VehicleId);
    }

    [Fact]
    public async Task AuthorizedStaffBypassesCustomerOwnershipFilter()
    {
        var query = new RecordingQuery();
        var ownership = new StubOwnership(false);
        var controller = CreateController(query, ownership, "ServiceAdvisor", null);

        var response = await controller.GetByVehicle(10, CancellationToken.None);

        Assert.IsType<OkObjectResult>(response.Result);
        Assert.Null(query.CustomerId);
        Assert.Equal(0, ownership.CallCount);
    }

    [Fact]
    public void ControllerRequiresAuthenticationAndPermittedRoles()
    {
        var authorize = typeof(ServiceHistoryController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal("Customer,ServiceAdvisor,Administrator", authorize.Roles);
    }

    private static ServiceHistoryController CreateController(
        RecordingQuery query,
        StubOwnership ownership,
        string role,
        string? customerId)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        if (customerId is not null)
            claims.Add(new Claim("customerId", customerId));

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
        context.Request.Headers.Authorization = "Bearer test-token";

        return new ServiceHistoryController(query, ownership)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class RecordingQuery : IServiceHistoryQueryService
    {
        public int CallCount { get; private set; }
        public int VehicleId { get; private set; }
        public int? CustomerId { get; private set; }

        public Task<VehicleServiceHistoryResponseDto> GetByVehicleAsync(
            int vehicleId,
            int? customerId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            VehicleId = vehicleId;
            CustomerId = customerId;
            return Task.FromResult(new VehicleServiceHistoryResponseDto { VehicleId = vehicleId });
        }
    }

    private sealed class StubOwnership(bool isOwned) : IVehicleOwnershipValidator
    {
        public bool IsOwned { get; set; } = isOwned;
        public int CallCount { get; private set; }

        public Task<bool> IsOwnedByCurrentCustomerAsync(
            int vehicleId,
            string bearerToken,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            Assert.Equal("test-token", bearerToken);
            return Task.FromResult(IsOwned);
        }
    }
}
