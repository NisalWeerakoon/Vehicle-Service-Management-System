using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize(Roles = "ServiceAdvisor,Administrator,Mechanic") ]
public class JobCardsController : ControllerBase
{
    private readonly JobMaintenanceDbContext _db;
    private readonly IJobCardService _jobCardService;

    public JobCardsController(JobMaintenanceDbContext db, IJobCardService jobCardService)
    {
        _db = db;
        _jobCardService = jobCardService;
    }

    [HttpPost]
    public async Task<ActionResult<JobCardResponseDto>> Create(
        [FromBody] CreateJobCardDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _jobCardService.CreateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobCardResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var jobs = await _db.JobCards
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new JobCardResponseDto
            {
                Id = x.Id,
                JobCardNumber = x.JobCardNumber,
                CheckInId = x.CheckInId,
                CustomerId = x.CustomerId,
                VehicleId = x.VehicleId,
                VehicleRegistrationNumber = x.VehicleRegistrationNumber,
                ReportedProblems = x.ReportedProblems,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(jobs);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobCardResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var job = await _db.JobCards
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (job is null)
            return NotFound(new { message = "Job card not found." });

        return Ok(JobCardService.ToResponse(job));
    }

    [HttpGet("check-in/{checkInId:int}")]
    public async Task<ActionResult<JobCardResponseDto>> GetByCheckIn(
        int checkInId,
        CancellationToken cancellationToken)
    {
        var job = await _db.JobCards
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CheckInId == checkInId, cancellationToken);

        if (job is null)
            return NotFound(new { message = "No job card exists for this check-in." });

        return Ok(JobCardService.ToResponse(job));
    }
}
