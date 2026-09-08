using System.Security.Claims;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Controllers;

[ApiController]
[Route("api/repair-tasks")]
[Authorize]
public class RepairTasksController : ControllerBase
{
    private readonly IRepairTaskService _service;
    public RepairTasksController(IRepairTaskService service) => _service = service;

    [HttpGet("job/{jobCardId:int}")]
    [Authorize(Roles = "Mechanic,ServiceAdvisor,Administrator")]
    public async Task<IActionResult> GetByJob(int jobCardId, CancellationToken cancellationToken)
    {
        try
        {
            var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isStaff = User.IsInRole("ServiceAdvisor") || User.IsInRole("Administrator");
            return Ok(await _service.GetByJobAsync(jobCardId, mechanicId, isStaff, cancellationToken));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(); }
    }

    [HttpPost]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> Create([FromBody] CreateRepairTaskDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(mechanicId)) return Unauthorized();
        var mechanicName = User.FindFirstValue(ClaimTypes.Email) ?? mechanicId;
        try { return Ok(await _service.CreateAsync(dto, mechanicId, mechanicName, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRepairTaskDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(mechanicId)) return Unauthorized();
        try { return Ok(await _service.UpdateAsync(id, dto, mechanicId, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(mechanicId)) return Unauthorized();
        try { return Ok(await _service.CompleteAsync(id, mechanicId, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
