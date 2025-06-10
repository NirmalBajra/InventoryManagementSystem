using System;

namespace InventoryManagementSystem.Dto;

public class PurchaseDto
{
    public int PurchaseId { get; set; }
    public int SupplierId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public List<PurchaseDetailDto> PurchaseDetails { get; set; }
}
