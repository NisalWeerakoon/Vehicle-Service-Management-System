namespace CustomerBookingService.Events;

public class BookingCreatedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();

    public string EventType { get; set; } = "BookingCreated";

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public string Source { get; set; } = "CustomerBookingService";

    public string? CorrelationId { get; set; }

    public BookingCreatedData Data { get; set; } = new();
}

public class BookingCreatedData
{
    public int BookingId { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public int VehicleId { get; set; }

    public string VehicleRegistrationNumber { get; set; } = string.Empty;

    public DateTime PreferredDate { get; set; }

    public string RequestedServiceOrProblem { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}