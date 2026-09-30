using System.ComponentModel.DataAnnotations;

namespace InventoryService.DTOs;

public class CreateSparePartDto
{
    [Required, MinLength(2), MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(2), MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal UnitPrice { get; set; }
}

public class UpdateSparePartDto : CreateSparePartDto
{
}

public class AdjustStockDto
{
    [Range(-2147483648, 2147483647)]
    public int Adjustment { get; set; }
}

public class SparePartResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StockReportItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsLowStock { get; set; }
    public string StockStatus => IsLowStock ? "LOW STOCK" : "NORMAL";
}
