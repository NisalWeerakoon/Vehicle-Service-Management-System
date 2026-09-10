using System.Security.Claims;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Controllers;

[ApiController]
[Route("api/job-status")]
[Authorize]
public class JobStatusController : ControllerBase
{
    private readonly IJobStatusService _service;

    public JobStatusController(IJobStatusService service)
    {
        _service = service;
    }

    [HttpGet("{jobCardId:int}")]
    public async Task<ActionResult<JobStatusResponseDto>> GetStatus(
        int jobCardId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetStatusAsync(jobCardId, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{jobCardId:int}/history")]
    public async Task<ActionResult<IReadOnlyList<JobStatusHistoryDto>>> GetHistory(
        int jobCardId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetHistoryAsync(jobCardId, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{jobCardId:int}/transition")]
    [Authorize(Roles = "ServiceAdvisor,Administrator,Mechanic")]
    public async Task<ActionResult<JobStatusResponseDto>> Transition(
        int jobCardId,
        [FromBody] UpdateJobStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            string.Empty;

        var role =
            User.FindFirstValue(ClaimTypes.Role) ??
            User.FindFirstValue("role") ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
            return Forbid();

        try
        {
            return Ok(await _service.UpdateStatusAsync(
                jobCardId, dto.Status, userId, role, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
