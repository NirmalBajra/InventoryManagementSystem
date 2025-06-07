using System;

namespace InventoryManagementSystem.Dto;

public class PurchaseDto
{
    public int SupplierId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public List<PurchaseDetailDto> PurchaseDetails { get; set; }
}
