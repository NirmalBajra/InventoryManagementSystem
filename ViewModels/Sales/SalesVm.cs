using System;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.ViewModels.Product;

namespace InventoryManagementSystem.ViewModels.Sales;

public class SalesVm
{
    public int SalesId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime SalesDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public List<SalesDetailVm> SalesDetails { get; set; } = new List<SalesDetailVm>();
    public List<ProductVm> Products { get; set; } = new List<ProductVm>();
}
