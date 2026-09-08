using System.Security.Claims;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Controllers;

[ApiController]
[Route("api/inspections")]
[Authorize]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionService _service;
    public InspectionsController(IInspectionService service) => _service = service;

    [HttpPost]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> Save([FromBody] CreateInspectionDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(mechanicId)) return Unauthorized();
        var mechanicName = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? mechanicId;
        try { return Ok(await _service.SaveAsync(dto, mechanicId, mechanicName, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(mechanicId)) return Unauthorized();
        return Ok(await _service.GetMyAsync(mechanicId, cancellationToken));
    }

    [HttpGet("completed")]
    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    public async Task<IActionResult> GetCompleted(CancellationToken cancellationToken) =>
        Ok(await _service.GetCompletedAsync(cancellationToken));

    [HttpGet("job/{jobCardId:int}")]
    [Authorize(Roles = "Mechanic,ServiceAdvisor,Administrator")]
    public async Task<IActionResult> GetByJob(int jobCardId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByJobAsync(jobCardId, cancellationToken);
        if (result is null) return NotFound(new { message = "No inspection exists for this job card." });
        if (User.IsInRole("Mechanic"))
        {
            var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (result.MechanicId != mechanicId) return Forbid();
        }
        else if (!result.IsCompleted)
        {
            return Forbid();
        }
        return Ok(result);
    }

    [HttpPost("{inspectionId:int}/complete")]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> Complete(int inspectionId, CancellationToken cancellationToken)
    {
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(mechanicId)) return Unauthorized();
        try { return Ok(await _service.CompleteAsync(inspectionId, mechanicId, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
