using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Models;
using JobMaintenanceService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JobMaintenanceService.Tests;

public class MechanicAssignmentServiceTests
{
    [Theory]
    [InlineData("Mechanic")]
    [InlineData("mechanic")]
    public async Task AssignAsync_ValidMechanic_PersistsAssignmentAndForwardsBearerToken(string role)
    {
        await using var fixture = await Fixture.CreateAsync(role: role);

        var result = await fixture.Service.AssignAsync(
            new AssignMechanicDto { JobCardId = fixture.Job.Id, MechanicId = "42" },
            "advisor-7",
            "test-token");

        Assert.Equal(fixture.Job.Id, result.JobCardId);
        Assert.Equal("42", result.MechanicId);
        Assert.Equal("mechanic@example.test", result.MechanicName);
        Assert.Equal("advisor-7", result.AssignedBy);
        Assert.True(result.IsActive);
        Assert.Equal(1, fixture.Handler.CallCount);
        Assert.Equal("http://customer-booking.test/api/auth/staff/42", fixture.Handler.RequestUri?.ToString());
        Assert.Equal(new AuthenticationHeaderValue("Bearer", "test-token"), fixture.Handler.Authorization);
        Assert.Single(await fixture.Db.MechanicAssignments.ToListAsync());
    }

    [Theory]
    [InlineData("ServiceAdvisor", true, HttpStatusCode.OK)]
    [InlineData("Mechanic", false, HttpStatusCode.OK)]
    [InlineData("Mechanic", true, HttpStatusCode.NotFound)]
    public async Task AssignAsync_InvalidStaff_PreservesExistingAssignment(
        string role, bool isActive, HttpStatusCode responseStatus)
    {
        await using var fixture = await Fixture.CreateAsync(role, isActive, responseStatus);
        var previous = new MechanicAssignment
        {
            JobCardId = fixture.Job.Id,
            MechanicId = "17",
            MechanicName = "previous@example.test",
            AssignedBy = "advisor-1",
            IsActive = true
        };
        fixture.Db.MechanicAssignments.Add(previous);
        await fixture.Db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.AssignAsync(
            new AssignMechanicDto { JobCardId = fixture.Job.Id, MechanicId = "42" },
            "advisor-7",
            "test-token"));

        Assert.True(previous.IsActive);
        Assert.Single(await fixture.Db.MechanicAssignments.ToListAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-number")]
    [InlineData("0")]
    [InlineData("-5")]
    public async Task AssignAsync_InvalidMechanicId_DoesNotCallStaffService(string mechanicId)
    {
        await using var fixture = await Fixture.CreateAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.AssignAsync(
            new AssignMechanicDto { JobCardId = fixture.Job.Id, MechanicId = mechanicId },
            "advisor-7",
            "test-token"));

        Assert.Equal(0, fixture.Handler.CallCount);
        Assert.Empty(await fixture.Db.MechanicAssignments.ToListAsync());
    }

    [Fact]
    public async Task AssignAsync_UnknownJob_DoesNotCallStaffService()
    {
        await using var fixture = await Fixture.CreateAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => fixture.Service.AssignAsync(
            new AssignMechanicDto { JobCardId = fixture.Job.Id + 1, MechanicId = "42" },
            "advisor-7",
            "test-token"));

        Assert.Equal(0, fixture.Handler.CallCount);
    }

    [Fact]
    public async Task AssignAsync_ValidReplacement_DeactivatesPreviousMechanic()
    {
        await using var fixture = await Fixture.CreateAsync();
        var previous = new MechanicAssignment
        {
            JobCardId = fixture.Job.Id,
            MechanicId = "17",
            MechanicName = "previous@example.test",
            AssignedBy = "advisor-1",
            IsActive = true
        };
        fixture.Db.MechanicAssignments.Add(previous);
        await fixture.Db.SaveChangesAsync();

        var result = await fixture.Service.AssignAsync(
            new AssignMechanicDto { JobCardId = fixture.Job.Id, MechanicId = "42" },
            "advisor-7",
            "test-token");

        Assert.False(previous.IsActive);
        Assert.True(result.IsActive);
        Assert.Equal(2, await fixture.Db.MechanicAssignments.CountAsync());
        Assert.Single(await fixture.Service.GetByJobAsync(fixture.Job.Id));
        Assert.Empty(await fixture.Service.GetByMechanicAsync("17"));
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private Fixture(
            JobMaintenanceDbContext db,
            JobCard job,
            RecordingHandler handler,
            HttpClient client,
            MechanicAssignmentService service)
        {
            Db = db;
            Job = job;
            Handler = handler;
            Client = client;
            Service = service;
        }

        public JobMaintenanceDbContext Db { get; }
        public JobCard Job { get; }
        public RecordingHandler Handler { get; }
        public HttpClient Client { get; }
        public MechanicAssignmentService Service { get; }

        public static async Task<Fixture> CreateAsync(
            string role = "Mechanic",
            bool isActive = true,
            HttpStatusCode responseStatus = HttpStatusCode.OK)
        {
            var options = new DbContextOptionsBuilder<JobMaintenanceDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new JobMaintenanceDbContext(options);
            var job = new JobCard
            {
                JobCardNumber = $"JC-{Guid.NewGuid():N}"[..19].ToUpperInvariant(),
                CheckInId = Random.Shared.Next(1, int.MaxValue),
                CustomerId = 31,
                VehicleId = 42,
                VehicleRegistrationNumber = "CAB-1234",
                ReportedProblems = "Brake noise"
            };
            db.JobCards.Add(job);
            await db.SaveChangesAsync();

            // This handler is a test double for Customer Booking's staff API.
            // Tests neither start that service nor make an external HTTP request.
            var handler = new RecordingHandler(responseStatus, role, isActive);
            var client = new HttpClient(handler);
            var configuration = new ConfigurationManager
            {
                ["Services:CustomerBookingServiceUrl"] = "http://customer-booking.test"
            };
            var service = new MechanicAssignmentService(
                db,
                new StubHttpClientFactory(client),
                configuration);
            return new Fixture(db, job, handler, client, service);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Db.DisposeAsync();
        }
    }

    private sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal("CustomerBookingService", name);
            return client;
        }
    }

    private sealed class RecordingHandler(
        HttpStatusCode responseStatus,
        string role,
        bool isActive) : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public Uri? RequestUri { get; private set; }
        public AuthenticationHeaderValue? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            RequestUri = request.RequestUri;
            Authorization = request.Headers.Authorization;
            var response = new HttpResponseMessage(responseStatus);
            if (responseStatus == HttpStatusCode.OK)
            {
                var staff = new StaffValidationResponseDto
                {
                    UserId = 42,
                    Email = "mechanic@example.test",
                    Role = role,
                    IsActive = isActive
                };
                response.Content = new StringContent(JsonSerializer.Serialize(staff));
            }

            return Task.FromResult(response);
        }
    }
}
