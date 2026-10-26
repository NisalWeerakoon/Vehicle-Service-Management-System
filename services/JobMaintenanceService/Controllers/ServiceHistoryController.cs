using System.Security.Claims;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Controllers;

[ApiController]
[Route("api/service-history")]
[Authorize(Roles = "Customer,ServiceAdvisor,Administrator")]
public class ServiceHistoryController(
    IServiceHistoryQueryService historyQuery,
    IVehicleOwnershipValidator ownershipValidator) : ControllerBase
{
    [HttpGet("vehicles/{vehicleId:int}")]
    public async Task<ActionResult<VehicleServiceHistoryResponseDto>> GetByVehicle(
        int vehicleId,
        CancellationToken cancellationToken)
    {
        if (vehicleId <= 0)
            return BadRequest(new { message = "A valid vehicle ID is required." });

        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
        int? customerId = null;

        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(User.FindFirstValue("customerId"), out var parsedCustomerId) ||
                parsedCustomerId <= 0)
                return Forbid();

            var authorization = Request.Headers.Authorization.ToString();
            const string bearerPrefix = "Bearer ";
            if (!authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { message = "A bearer token is required." });

            var bearerToken = authorization[bearerPrefix.Length..].Trim();

            try
            {
                if (!await ownershipValidator.IsOwnedByCurrentCustomerAsync(
                        vehicleId,
                        bearerToken,
                        cancellationToken))
                    return NotFound(new { message = "Vehicle was not found." });
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new { message = "Vehicle ownership could not be verified. Please try again." });
            }

            customerId = parsedCustomerId;
        }

        return Ok(await historyQuery.GetByVehicleAsync(
            vehicleId,
            customerId,
            cancellationToken));
    }
}
