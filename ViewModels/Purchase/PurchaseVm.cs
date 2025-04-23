using System;

namespace InventoryManagementSystem.ViewModels.Purchase;

public class PurchaseVm
{
    public int SupplierId { get; set;}
    public DateTime CreatedAt { get; set;}
    public List<PurchaseDetailVm> PurchaseDetails { get; set;}
}
