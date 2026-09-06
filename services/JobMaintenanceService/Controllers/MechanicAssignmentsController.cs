using System.Security.Claims;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Controllers;

[ApiController]
[Route("api/mechanic-assignments")]
[Authorize]
public class MechanicAssignmentsController : ControllerBase
{
    private readonly IMechanicAssignmentService _service;

    public MechanicAssignmentsController(IMechanicAssignmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    public async Task<IActionResult> Assign(
        [FromBody] AssignMechanicDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var assignedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.Identity?.Name
                         ?? "Unknown";

        var token = Request.Headers.Authorization.ToString();
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            token = token[7..];

        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized();

        try
        {
            var result = await _service.AssignAsync(
                dto,
                assignedBy,
                token,
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my-jobs")]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> GetMyJobs(
        CancellationToken cancellationToken)
    {
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(mechanicId))
            return Unauthorized();

        var result = await _service.GetByMechanicAsync(
            mechanicId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("job/{jobCardId:int}")]
    [Authorize(Roles = "ServiceAdvisor,Administrator,Mechanic")]
    public async Task<IActionResult> GetByJob(
        int jobCardId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByJobAsync(
            jobCardId,
            cancellationToken);

        if (User.IsInRole("Mechanic"))
        {
            var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ownsAssignment = result.Any(x => x.MechanicId == mechanicId);

            if (!ownsAssignment)
                return Forbid();
        }

        return Ok(result);
    }
}
