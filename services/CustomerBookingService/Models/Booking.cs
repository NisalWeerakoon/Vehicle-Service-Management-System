using System.ComponentModel.DataAnnotations;

namespace CustomerBookingService.Models;

public class Booking
{
    public int Id { get; set; }

    // Human-friendly booking reference
    [Required]
    [MaxLength(30)]
    public string BookingReference { get; set; } = string.Empty;

    // Customer who owns this booking
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    // Vehicle being serviced
    public int VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;

    [Required]
    public DateTime PreferredDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string RequestedServiceOrProblem { get; set; } = string.Empty;

    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}