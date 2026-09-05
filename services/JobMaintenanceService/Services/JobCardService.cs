using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IJobCardService
{
    Task<JobCardResponseDto> CreateAsync(CreateJobCardDto dto, CancellationToken cancellationToken = default);
}

public class JobCardService : IJobCardService
{
    private readonly JobMaintenanceDbContext _db;

    public JobCardService(JobMaintenanceDbContext db)
    {
        _db = db;
    }

    public async Task<JobCardResponseDto> CreateAsync(
        CreateJobCardDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.JobCards
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CheckInId == dto.CheckInId, cancellationToken);

        if (existing is not null)
        {
            return ToResponse(existing);
        }

        var jobCard = new JobCard
        {
            JobCardNumber = $"JC-{Guid.NewGuid():N}"[..19].ToUpperInvariant(),
            CheckInId = dto.CheckInId,
            CustomerId = dto.CustomerId,
            VehicleId = dto.VehicleId,
            VehicleRegistrationNumber = dto.VehicleRegistrationNumber.Trim().ToUpperInvariant(),
            ReportedProblems = dto.ReportedProblems.Trim(),
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        _db.JobCards.Add(jobCard);
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(jobCard);
    }

    public static JobCardResponseDto ToResponse(JobCard job) => new()
    {
        Id = job.Id,
        JobCardNumber = job.JobCardNumber,
        CheckInId = job.CheckInId,
        CustomerId = job.CustomerId,
        VehicleId = job.VehicleId,
        VehicleRegistrationNumber = job.VehicleRegistrationNumber,
        ReportedProblems = job.ReportedProblems,
        Status = job.Status,
        CreatedAt = job.CreatedAt,
        UpdatedAt = job.UpdatedAt
    };
}
