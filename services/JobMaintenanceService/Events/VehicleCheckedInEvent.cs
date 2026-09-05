namespace JobMaintenanceService.Events;

public class VehicleCheckedInEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public VehicleCheckedInData Data { get; set; } = new();
}

public class VehicleCheckedInData
{
    public int CheckInId { get; set; }
    public int? BookingId { get; set; }
    public string? BookingReference { get; set; }
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public DateTime CheckInDateTime { get; set; }
    public int Mileage { get; set; }
    public string ReportedProblems { get; set; } = string.Empty;
    public bool IsWalkIn { get; set; }
    public string ServiceStatus { get; set; } = string.Empty;
}
