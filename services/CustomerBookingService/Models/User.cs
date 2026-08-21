using System.ComponentModel.DataAnnotations;

namespace CustomerBookingService.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;

    public bool IsActive { get; set; } = true;

    // Only Customer accounts use this relationship.
    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}