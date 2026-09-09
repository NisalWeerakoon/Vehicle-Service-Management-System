namespace JobMaintenanceService.Events;

public class ServiceCompletedEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = "ServiceCompleted";
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = "JobMaintenanceService";
    public string? CorrelationId { get; set; }
    public ServiceCompletedData Data { get; set; } = new();
}

public class ServiceCompletedData
{
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public string CompletedBy { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}

public class VehicleReadyForCollectionEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = "VehicleReadyForCollection";
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = "JobMaintenanceService";
    public string? CorrelationId { get; set; }
    public VehicleReadyForCollectionData Data { get; set; } = new();
}

public class VehicleReadyForCollectionData
{
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public string ReadyBy { get; set; } = string.Empty;
    public DateTime ReadyAt { get; set; }
}
