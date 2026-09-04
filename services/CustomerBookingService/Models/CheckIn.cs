using System.ComponentModel.DataAnnotations;

namespace CustomerBookingService.Models;

public class CheckIn
{
    public int Id { get; set; }

    // Null is allowed only for a purely standalone check-in,
    // but the current walk-in flow creates a booking automatically.
    public int? BookingId { get; set; }

    public Booking? Booking { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public int VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;

    [Required]
    public DateTime CheckInDateTime { get; set; } = DateTime.UtcNow;

    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }

    [Required]
    [MaxLength(500)]
    public string ReportedProblems { get; set; } = string.Empty;

    // A check-in remains active until the service workflow moves the vehicle on.
    // This prevents a second active check-in for the same booking/vehicle.
    public bool IsActive { get; set; } = true;

    public bool IsWalkIn { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
