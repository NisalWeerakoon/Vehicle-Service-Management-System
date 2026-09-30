namespace JobMaintenanceService.Models;

public class PartRequest
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public int SparePartId { get; set; }
    public int RequestedQuantity { get; set; }
    public string RequestingMechanicId { get; set; } = string.Empty;
    public string RequestingMechanicName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
