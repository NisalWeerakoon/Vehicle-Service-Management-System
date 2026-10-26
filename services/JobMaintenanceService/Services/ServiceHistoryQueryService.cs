using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IServiceHistoryQueryService
{
    Task<VehicleServiceHistoryResponseDto> GetByVehicleAsync(
        int vehicleId,
        int? customerId,
        CancellationToken cancellationToken = default);
}

public class ServiceHistoryQueryService(
    JobMaintenanceDbContext db,
    ILogger<ServiceHistoryQueryService> logger) : IServiceHistoryQueryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<VehicleServiceHistoryResponseDto> GetByVehicleAsync(
        int vehicleId,
        int? customerId,
        CancellationToken cancellationToken = default)
    {
        var query = db.ServiceHistories.AsNoTracking()
            .Where(x => x.VehicleId == vehicleId);

        if (customerId.HasValue)
            query = query.Where(x => x.CustomerId == customerId.Value);

        var snapshots = await query
            .OrderByDescending(x => x.CompletedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return new VehicleServiceHistoryResponseDto
        {
            VehicleId = vehicleId,
            VehicleRegistrationNumber = snapshots.FirstOrDefault()?.VehicleRegistrationNumber,
            Records = snapshots.Select(ToResponse).ToList()
        };
    }

    private ServiceHistoryResponseDto ToResponse(Models.ServiceHistory history)
    {
        var inspection = history.InspectionResults is null &&
                         history.IdentifiedProblems is null &&
                         history.InspectionCompletedAt is null
            ? null
            : new InspectionHistoryDto
            {
                InspectionResults = history.InspectionResults,
                IdentifiedProblems = history.IdentifiedProblems,
                CompletedAt = history.InspectionCompletedAt
            };

        return new ServiceHistoryResponseDto
        {
            Id = history.Id,
            JobCardId = history.JobCardId,
            JobCardNumber = history.JobCardNumber,
            VehicleId = history.VehicleId,
            VehicleRegistrationNumber = history.VehicleRegistrationNumber,
            CompletedAt = history.CompletedAt,
            CompletedBy = history.CompletedBy,
            FinalStatus = history.FinalStatus,
            ReportedProblems = history.ReportedProblems,
            CompletedRepairTasks = DeserializeList<CompletedRepairTaskHistoryDto>(
                history.CompletedRepairTasksJson,
                history.Id,
                "completed repair tasks"),
            RepairNotes = DeserializeList<RepairNoteHistoryDto>(
                history.RepairNotesJson,
                history.Id,
                "repair notes"),
            Inspection = inspection
        };
    }

    private List<T> DeserializeList<T>(string json, int historyId, string field)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];
        }
        catch (JsonException ex)
        {
            logger.LogWarning(
                ex,
                "Service history {HistoryId} contains invalid {Field} JSON.",
                historyId,
                field);
            return [];
        }
    }
}

public interface IVehicleOwnershipValidator
{
    Task<bool> IsOwnedByCurrentCustomerAsync(
        int vehicleId,
        string bearerToken,
        CancellationToken cancellationToken = default);
}

public class VehicleOwnershipValidator(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IVehicleOwnershipValidator
{
    public async Task<bool> IsOwnedByCurrentCustomerAsync(
        int vehicleId,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["Services:CustomerBookingServiceUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Customer Booking Service URL is not configured.");

        var client = httpClientFactory.CreateClient("CustomerBookingService");
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/vehicles/me/{vehicleId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return true;

        if (response.StatusCode is HttpStatusCode.NotFound or
            HttpStatusCode.BadRequest or
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden)
            return false;

        response.EnsureSuccessStatusCode();
        return false;
    }
}
