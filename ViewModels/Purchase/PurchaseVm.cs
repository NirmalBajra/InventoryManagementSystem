using System;
using InventoryManagementSystem.ViewModels.Product;
using InventoryManagementSystem.ViewModels.Suppliers;

namespace InventoryManagementSystem.ViewModels.Purchase;

public class PurchaseVm
{
    public int PurchaseId { get; set; }
    public int SupplierId { get; set; }
    public DateTime CreatedAt { get; set;}
    public List<PurchaseDetailVm> PurchaseDetails { get; set;} = new List<PurchaseDetailVm>();
    public List<SupplierVm> Suppliers { get; set; }
}
