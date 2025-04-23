using System;

namespace InventoryManagementSystem.ViewModels.Suppliers;

public class SupplierVm
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
    public string Contact { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
