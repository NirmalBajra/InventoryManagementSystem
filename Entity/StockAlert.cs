using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class StockAlert
{
    [Key]
    public int AlertId { get; set; }
    public int ProductId { get; set; }
    public string AlertType { get; set; } = string.Empty; // "LowStock", "OutOfStock", "Expiring"
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    
    // Navigation property
    public Product Product { get; set; } = null!;
} 