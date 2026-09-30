using BillingService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BillingService.DTOs;
using BillingService.Models;
using BillingService.Services;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Controllers;
[ApiController, Route("api/part-charges"), Authorize]
public class PartChargesController(BillingDbContext db, IInvoiceService invoices) : ControllerBase
{
    [HttpGet("job/{jobCardId:int}")]
    public async Task<IActionResult> GetByJob(int jobCardId, CancellationToken ct) => Ok(await db.PartCharges.AsNoTracking().Where(x => x.JobCardId == jobCardId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct));

    [HttpGet("invoice/job/{jobCardId:int}")]
    public async Task<IActionResult> GetInvoice(int jobCardId, CancellationToken ct) => Ok(await invoices.GetByJobAsync(jobCardId, ct));

    [HttpPost("invoice/job/{jobCardId:int}/service"), Authorize(Roles = "Accounts,Administrator")]
    public async Task<IActionResult> AddService(int jobCardId, AddManualChargeDto dto, CancellationToken ct) => await AddManual(jobCardId, ChargeType.Service, dto, ct);
    [HttpPost("invoice/job/{jobCardId:int}/labour"), Authorize(Roles = "Accounts,Administrator")]
    public async Task<IActionResult> AddLabour(int jobCardId, AddManualChargeDto dto, CancellationToken ct) => await AddManual(jobCardId, ChargeType.Labour, dto, ct);
    private async Task<IActionResult> AddManual(int jobCardId, ChargeType type, AddManualChargeDto dto, CancellationToken ct) { if (!ModelState.IsValid) return ValidationProblem(ModelState); try { return Ok(await invoices.AddManualAsync(jobCardId, type, dto, ct)); } catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); } }
}
