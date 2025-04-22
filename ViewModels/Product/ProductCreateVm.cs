using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Product;

public class ProductCreateVm
{
    [Required]
    public string ProductName { get; set;}
    public string Description { get; set;}
    [Required]
    public int CategoryId { get; set;}
    public IFormFile? ImageFile { get; set; }
}
