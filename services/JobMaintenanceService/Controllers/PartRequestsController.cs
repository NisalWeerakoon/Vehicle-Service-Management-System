using System.Security.Claims;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Controllers;

[ApiController, Route("api/job-part-requests"), Authorize(Roles = "Mechanic")]
public class PartRequestsController(IPartRequestService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreatePartRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try { var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException(); var name = User.FindFirstValue(ClaimTypes.Email) ?? id; var result = await service.CreateAsync(dto, id, name, ct); return CreatedAtAction(nameof(GetMine), result); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct) => Ok(await service.GetMineAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException(), ct));
}
