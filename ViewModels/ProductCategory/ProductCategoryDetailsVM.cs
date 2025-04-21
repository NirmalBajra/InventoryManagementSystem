using System;

namespace InventoryManagementSystem.ViewModels.ProductCategory;

public class ProductCategoryDetailsVM
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Possibly include related Products
    public List<string> Products { get; set; }
}
