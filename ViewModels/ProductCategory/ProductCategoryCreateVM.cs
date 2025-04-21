using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.ProductCategory;

public class ProductCategoryCreateVM
{
    [Required]
    public string CategoryName { get; set; }
    public string Description { get; set; }
}
