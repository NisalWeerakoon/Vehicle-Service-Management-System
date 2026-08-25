using System.ComponentModel.DataAnnotations;

namespace CustomerBookingService.Models;

public class Vehicle
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    [Required]
    [MaxLength(40)]
    public string FuelType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}