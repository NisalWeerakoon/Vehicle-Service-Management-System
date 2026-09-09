using System.Security.Claims;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Controllers;

[ApiController]
[Route("api/repair-notes")]
[Authorize]
public class RepairNotesController : ControllerBase
{
    private readonly IRepairNoteService _service;
    public RepairNotesController(IRepairNoteService service) => _service = service;

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
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> Create([FromBody] CreateRepairNoteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(mechanicId)) return Unauthorized();
        var mechanicName = User.FindFirstValue(ClaimTypes.Email) ?? mechanicId;
        try { return Ok(await _service.CreateAsync(dto, mechanicId, mechanicName, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRepairNoteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(mechanicId)) return Unauthorized();
        try { return Ok(await _service.UpdateAsync(id, dto, mechanicId, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
