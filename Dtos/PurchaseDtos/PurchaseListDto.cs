using System;
using System.Collections.Generic;
using InventoryManagementSystem.Dto;

namespace InventoryManagementSystem.Dtos.PurchaseDtos;

public class PurchaseListDto
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public string SupplierName { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseDetailDto> PurchaseDetails { get; set; } = new List<PurchaseDetailDto>();
}

