using System;

namespace InventoryManagementSystem.Dtos.SalesDtos;

public class CreateSalesDto
{
    public string CustomerName { get; set; }
    public DateTime SalesDate { get; set; }
    public List<SalesDetailDto> SalesDetails { get; set; }

    public decimal TotalAmount { get; set; }
}
