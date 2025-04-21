using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.ProductCategory;

public class ProductCategoryUpdateVM
{
    [Required]
    public int CategoryId { get; set; }
    [Required]
    public string CategoryName { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}
