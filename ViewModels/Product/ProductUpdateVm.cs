using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Product;

public class ProductUpdate
{
    [Required]
    public int ProductId { get; set;}
    public string ProductName { get; set;}
    public string Description { get; set;}
    [Required]
    public int CategoryId { get; set;}
}
