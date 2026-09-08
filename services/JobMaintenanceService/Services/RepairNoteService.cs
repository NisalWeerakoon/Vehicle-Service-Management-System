using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IRepairNoteService
{
    Task<List<RepairNoteResponseDto>> GetByJobAsync(int jobCardId, string? mechanicId, bool isStaff, CancellationToken cancellationToken = default);
    Task<RepairNoteResponseDto> CreateAsync(CreateRepairNoteDto dto, string mechanicId, string mechanicName, CancellationToken cancellationToken = default);
    Task<RepairNoteResponseDto> UpdateAsync(int id, UpdateRepairNoteDto dto, string mechanicId, CancellationToken cancellationToken = default);
}

public class RepairNoteService : IRepairNoteService
{
    private readonly JobMaintenanceDbContext _db;
    public RepairNoteService(JobMaintenanceDbContext db) => _db = db;

    public async Task<List<RepairNoteResponseDto>> GetByJobAsync(int jobCardId, string? mechanicId, bool isStaff, CancellationToken cancellationToken = default)
    {
        await EnsureJobExistsAsync(jobCardId, cancellationToken);
        if (!isStaff && !await IsAssignedAsync(jobCardId, mechanicId!, cancellationToken)) throw new UnauthorizedAccessException("You are not assigned to this job card.");
        return await Query().Where(x => x.JobCardId == jobCardId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<RepairNoteResponseDto> CreateAsync(CreateRepairNoteDto dto, string mechanicId, string mechanicName, CancellationToken cancellationToken = default)
    {
        Validate(dto.Note);
        await EnsureAssignedAsync(dto.JobCardId, mechanicId, cancellationToken);
        var note = new RepairNote { JobCardId = dto.JobCardId, MechanicId = mechanicId, MechanicName = mechanicName, Note = dto.Note.Trim(), CreatedAt = DateTime.UtcNow };
        _db.RepairNotes.Add(note); await _db.SaveChangesAsync(cancellationToken); return await BuildResponseAsync(note.Id, cancellationToken);
    }

    public async Task<RepairNoteResponseDto> UpdateAsync(int id, UpdateRepairNoteDto dto, string mechanicId, CancellationToken cancellationToken = default)
    {
        Validate(dto.Note);
        var note = await _db.RepairNotes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new KeyNotFoundException("Repair note not found.");
        if (note.MechanicId != mechanicId) throw new UnauthorizedAccessException("You can only update your own repair notes.");
        await EnsureAssignedAsync(note.JobCardId, mechanicId, cancellationToken);
        note.Note = dto.Note.Trim(); note.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(cancellationToken); return await BuildResponseAsync(id, cancellationToken);
    }

    private IQueryable<RepairNoteResponseDto> Query() => from note in _db.RepairNotes.AsNoTracking() join job in _db.JobCards.AsNoTracking() on note.JobCardId equals job.Id select new RepairNoteResponseDto { Id = note.Id, JobCardId = note.JobCardId, JobCardNumber = job.JobCardNumber, MechanicId = note.MechanicId, MechanicName = note.MechanicName, Note = note.Note, CreatedAt = note.CreatedAt, UpdatedAt = note.UpdatedAt };
    private async Task<RepairNoteResponseDto> BuildResponseAsync(int id, CancellationToken cancellationToken) => await Query().FirstAsync(x => x.Id == id, cancellationToken);
    private async Task EnsureAssignedAsync(int jobCardId, string mechanicId, CancellationToken cancellationToken) { await EnsureJobExistsAsync(jobCardId, cancellationToken); if (!await IsAssignedAsync(jobCardId, mechanicId, cancellationToken)) throw new UnauthorizedAccessException("You are not assigned to this job card."); }
    private async Task<bool> IsAssignedAsync(int jobCardId, string mechanicId, CancellationToken cancellationToken) => await _db.MechanicAssignments.AnyAsync(x => x.JobCardId == jobCardId && x.MechanicId == mechanicId && x.IsActive, cancellationToken);
    private async Task EnsureJobExistsAsync(int jobCardId, CancellationToken cancellationToken) { if (!await _db.JobCards.AnyAsync(x => x.Id == jobCardId, cancellationToken)) throw new KeyNotFoundException("Job card not found."); }
    private static void Validate(string? value) { if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 3) throw new ArgumentException("Note is required and must contain at least 3 characters."); if (value.Trim().Length > 2000) throw new ArgumentException("Note cannot exceed 2000 characters."); }
}
