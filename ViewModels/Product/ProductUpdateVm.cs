using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Product;

public class ProductUpdateVm
{
    [Required]
    public int ProductId { get; set;}
    public string? ProductName { get; set;}
    public string? Description { get; set;}
    [Required]
    public int CategoryId { get; set;}
    public string? ImagePath { get; set; }
    public IFormFile? ImageFile { get; set; }

}
