using System;

namespace InventoryManagementSystem.Dtos.SalesDtos;

public class SalesDetailDto
{
    public int ProductId { get; set;}
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
