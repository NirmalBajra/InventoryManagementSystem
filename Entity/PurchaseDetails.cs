using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class PurchaseDetails
{
    [Key]
    public int ProductDetailId { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CategoryId { get; set; }
    public ProductCategory Category { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; }
}
