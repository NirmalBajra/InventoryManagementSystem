using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    public string ProductName { get; set; }
    public string Description { get; set; }
    
    public int CategoryId { get; set; }
    public ProductCategory Category { get; set; }

    // Product Image path 
    public string ImagePath { get; set; }

}