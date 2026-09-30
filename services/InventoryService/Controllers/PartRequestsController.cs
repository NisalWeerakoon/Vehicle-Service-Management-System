using System.Security.Claims;
using InventoryService.DTOs;
using InventoryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/part-requests")]
[Authorize]
public class PartRequestsController : ControllerBase
{
    private readonly IPartRequestService _service;
    public PartRequestsController(IPartRequestService service) => _service = service;

    [HttpPost, Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> Create(CreatePartRequestDto dto, CancellationToken ct) => await Execute(async () =>
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var name = User.FindFirstValue(ClaimTypes.Email) ?? mechanicId;
        var result = await _service.CreateAsync(dto, mechanicId, name, Token(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    });

    [HttpGet("pending"), Authorize(Roles = "InventoryOfficer,Administrator")]
    public Task<IActionResult> GetPending(CancellationToken ct) => Execute(async () => Ok(await _service.GetPendingAsync(Token(), ct)));

    [HttpGet("mine"), Authorize(Roles = "Mechanic")]
    public Task<IActionResult> GetMine(CancellationToken ct) => Execute(async () =>
    {
        var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        return Ok(await _service.GetByMechanicAsync(mechanicId, Token(), ct));
    });

    [HttpGet("{id:int}"), Authorize(Roles = "Mechanic,InventoryOfficer,Administrator")]
    public Task<IActionResult> GetById(int id, CancellationToken ct) => Execute(async () =>
    {
        var result = await _service.GetByIdAsync(id, Token(), ct);
        if (User.IsInRole("Mechanic") && result.RequestingMechanicId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        return Ok(result);
    });

    [HttpPost("{id:int}/issue"), Authorize(Roles = "InventoryOfficer,Administrator")]
    public Task<IActionResult> Issue(int id, CancellationToken ct) => Execute(async () =>
    {
        var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var name = User.FindFirstValue(ClaimTypes.Email) ?? officerId;
        return Ok(await _service.IssueAsync(id, officerId, name, Token(), ct));
    });

    private string Token() => Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
    private async Task<IActionResult> Execute(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
