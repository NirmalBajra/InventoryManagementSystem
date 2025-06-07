using System;

namespace InventoryManagementSystem.Dto;

public class PurchaseDetailDto
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
