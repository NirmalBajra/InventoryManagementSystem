using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class ProductCategory
{
    [Key]
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; }
    public DateTime? CreatedAt { get; set; }

    public ICollection<Product> Products { get; set; }
    public ICollection<PurchaseDetails> PurchaseDetails { get; set; }
}
