using InventoryService.DTOs;
using InventoryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/spare-parts")]
[Authorize]
public class SparePartsController : ControllerBase
{
    private readonly ISparePartService _service;
    public SparePartsController(ISparePartService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "InventoryOfficer,Administrator,Mechanic")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(search, cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "InventoryOfficer,Administrator,Mechanic")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.GetByIdAsync(id, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet("reports/current-stock")]
    [Authorize(Roles = "InventoryOfficer,Administrator")]
    public async Task<IActionResult> GetCurrentStockReport(CancellationToken cancellationToken) =>
        Ok(await _service.GetCurrentStockReportAsync(cancellationToken));

    [HttpGet("reports/low-stock")]
    [Authorize(Roles = "InventoryOfficer,Administrator")]
    public async Task<IActionResult> GetLowStockReport(CancellationToken cancellationToken) =>
        Ok(await _service.GetLowStockReportAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "InventoryOfficer,Administrator")]
    public async Task<IActionResult> Create([FromBody] CreateSparePartDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var part = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "InventoryOfficer,Administrator")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSparePartDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try { return Ok(await _service.UpdateAsync(id, dto, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/adjust-stock")]
    [Authorize(Roles = "InventoryOfficer,Administrator")]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] AdjustStockDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try { return Ok(await _service.AdjustStockAsync(id, dto, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "InventoryOfficer,Administrator")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try { await _service.DeleteAsync(id, cancellationToken); return NoContent(); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
