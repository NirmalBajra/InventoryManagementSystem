using System;
using InventoryManagementSystem.Entity;

namespace InventoryManagementSystem.ViewModels.Sales;

public class SalesVm
{
    public string CustomerName { get; set; }
    public DateTime SalesDate { get; set; } = DateTime.UtcNow;
    public List<SalesDetailVm> SalesDetails { get; set; }
}
