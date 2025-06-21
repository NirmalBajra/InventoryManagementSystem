using System;

namespace InventoryManagementSystem.Dtos.StockDtos;

public class ViewStockDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public int? AvailableQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ImagePath { get; set; }
    public string CategoryName { get; set; }
}
