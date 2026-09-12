using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMaintenanceService.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "ServiceAdvisor,Administrator")]
public class ReportsController : ControllerBase
{
    private readonly IActiveJobsReportService _reportService;

    public ReportsController(IActiveJobsReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Returns the current active service workload, optionally filtered by status.
    /// </summary>
    [HttpGet("active-jobs")]
    public async Task<IActionResult> GetActiveJobs(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetActiveJobsAsync(
                status,
                cancellationToken);

            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
