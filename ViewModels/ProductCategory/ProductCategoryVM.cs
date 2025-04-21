using System;

namespace InventoryManagementSystem.ViewModels.ProductCategory;

public class ProductCategoryVM
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}
