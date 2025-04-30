using System;
using InventoryManagementSystem.ViewModels.Product;
using InventoryManagementSystem.ViewModels.Suppliers;

namespace InventoryManagementSystem.ViewModels.Purchase;

public class PurchaseListVm
{
    public int PurchaseId { get; set; }
    public int SupplierId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SupplierName { get; set; } 
    public decimal TotalAmount { get; set; }

    public List<PurchaseDetailVm> PurchaseDetails { get; set; } = new List<PurchaseDetailVm>();
    public List<SupplierVm> Suppliers { get; set; } = new List<SupplierVm>();
    public List<ProductVm> Products { get; set; } = new List<ProductVm>();
}
