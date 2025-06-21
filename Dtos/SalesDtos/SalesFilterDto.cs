using System;

namespace InventoryManagementSystem.Dtos.SalesDtos;

public class SalesFilterDto
{
    public string? CustomerName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortField { get; set; } = "SalesDate";
    public string? SortOrder { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
