using System.Net.Http.Headers;
using System.Text.Json;
using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IMechanicAssignmentService
{
    Task<MechanicAssignmentResponseDto> AssignAsync(
        AssignMechanicDto dto,
        string assignedBy,
        string bearerToken,
        CancellationToken cancellationToken = default);

    Task<List<MechanicAssignmentResponseDto>> GetByMechanicAsync(
        string mechanicId,
        CancellationToken cancellationToken = default);

    Task<List<MechanicAssignmentResponseDto>> GetByJobAsync(
        int jobCardId,
        CancellationToken cancellationToken = default);
}

public class MechanicAssignmentService : IMechanicAssignmentService
{
    private readonly JobMaintenanceDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public MechanicAssignmentService(
        JobMaintenanceDbContext db,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<MechanicAssignmentResponseDto> AssignAsync(
        AssignMechanicDto dto,
        string assignedBy,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        var job = await _db.JobCards
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dto.JobCardId, cancellationToken);

        if (job is null)
            throw new KeyNotFoundException("Job card not found.");

        var activeAssignment = await _db.MechanicAssignments
            .FirstOrDefaultAsync(
                x => x.JobCardId == dto.JobCardId && x.IsActive,
                cancellationToken);

        if (activeAssignment is not null)
            throw new InvalidOperationException(
                "A mechanic is already assigned to this job.");

        var staff = await ValidateMechanicAsync(
            dto.MechanicId,
            bearerToken,
            cancellationToken);

        if (staff is null || !staff.IsActive ||
            !string.Equals(staff.Role, "Mechanic", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The selected staff member is invalid, inactive, or is not a mechanic.");
        }

        var assignment = new MechanicAssignment
        {
            JobCardId = dto.JobCardId,
            MechanicId = staff.UserId.ToString(),
            MechanicName = staff.Email,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.MechanicAssignments.Add(assignment);
        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(assignment);
    }

    public async Task<List<MechanicAssignmentResponseDto>> GetByMechanicAsync(
        string mechanicId,
        CancellationToken cancellationToken = default)
    {
        return await _db.MechanicAssignments
            .AsNoTracking()
            .Where(x => x.MechanicId == mechanicId && x.IsActive)
            .OrderByDescending(x => x.AssignedAt)
            .Select(x => new MechanicAssignmentResponseDto
            {
                Id = x.Id,
                JobCardId = x.JobCardId,
                MechanicId = x.MechanicId,
                MechanicName = x.MechanicName,
                AssignedBy = x.AssignedBy,
                AssignedAt = x.AssignedAt,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MechanicAssignmentResponseDto>> GetByJobAsync(
        int jobCardId,
        CancellationToken cancellationToken = default)
    {
        return await _db.MechanicAssignments
            .AsNoTracking()
            .Where(x => x.JobCardId == jobCardId && x.IsActive)
            .Select(x => new MechanicAssignmentResponseDto
            {
                Id = x.Id,
                JobCardId = x.JobCardId,
                MechanicId = x.MechanicId,
                MechanicName = x.MechanicName,
                AssignedBy = x.AssignedBy,
                AssignedAt = x.AssignedAt,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<StaffValidationResponseDto?> ValidateMechanicAsync(
        string mechanicId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(mechanicId, out var id) || id <= 0)
            return null;

        var baseUrl = _configuration["Services:CustomerBookingServiceUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException(
                "Customer Booking Service URL is not configured.");

        var client = _httpClientFactory.CreateClient("CustomerBookingService");
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await client.GetAsync(
            $"api/auth/staff/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<StaffValidationResponseDto>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);
    }

    private static MechanicAssignmentResponseDto ToResponse(
        MechanicAssignment assignment) => new()
    {
        Id = assignment.Id,
        JobCardId = assignment.JobCardId,
        MechanicId = assignment.MechanicId,
        MechanicName = assignment.MechanicName,
        AssignedBy = assignment.AssignedBy,
        AssignedAt = assignment.AssignedAt,
        IsActive = assignment.IsActive
    };
}
