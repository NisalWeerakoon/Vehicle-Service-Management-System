using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace InventoryService.Services;

public interface IJobCardGateway
{
    Task<JobCardReference> GetJobAsync(int jobCardId, string bearerToken, CancellationToken cancellationToken = default);
    Task EnsureMechanicIsAssignedAsync(int jobCardId, string mechanicId, string bearerToken, CancellationToken cancellationToken = default);
}

public record JobCardReference(int Id, string JobCardNumber);

public class JobCardGateway : IJobCardGateway
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public JobCardGateway(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<JobCardReference> GetJobAsync(int jobCardId, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest($"api/jobs/{jobCardId}", bearerToken);
        using var response = await _httpClientFactory.CreateClient("JobMaintenanceService").SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) throw new KeyNotFoundException("Job card not found.");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Unable to validate the job card.");
        var job = await response.Content.ReadFromJsonAsync<JobCardReference>(JsonOptions, cancellationToken);
        return job ?? throw new InvalidOperationException("Unable to validate the job card.");
    }

    public async Task EnsureMechanicIsAssignedAsync(int jobCardId, string mechanicId, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest($"api/mechanic-assignments/job/{jobCardId}", bearerToken);
        using var response = await _httpClientFactory.CreateClient("JobMaintenanceService").SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Forbidden) throw new UnauthorizedAccessException("You are not assigned to this job card.");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Unable to validate the mechanic assignment.");
        var assignments = await response.Content.ReadFromJsonAsync<List<MechanicAssignmentReference>>(JsonOptions, cancellationToken) ?? [];
        if (!assignments.Any(x => x.MechanicId == mechanicId)) throw new UnauthorizedAccessException("You are not assigned to this job card.");
    }

    private static HttpRequestMessage CreateRequest(string path, string bearerToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return request;
    }

    private record MechanicAssignmentReference(string MechanicId);
}
