using System;

namespace InventoryManagementSystem.Dtos.PurchaseDtos;

public class PurchaseListDto
{
    public int Id { get; set; }
    public string SupplierName { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
}

