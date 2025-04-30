using System;

namespace InventoryManagementSystem.ViewModels.Purchase;

public class PurchaseDetailVm
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string CategoryName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
