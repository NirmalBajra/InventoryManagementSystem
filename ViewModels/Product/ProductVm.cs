using System;

namespace InventoryManagementSystem.ViewModels.Product;

public class ProductVm
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public string CategoryName { get; set; }
    public string ImagePath { get; set; }

    public int CategoryId { get; set; }
}
