using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class Supplier
{
    [Key]
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
    public string Contact { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Purchase> Purchases { get; set; }
}
