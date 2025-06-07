using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class Purchase
{
    [Key]
    public int PurchaseId { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }

    //Navigation Properties
    public ICollection<PurchaseDetails> PurchaseDetails { get; set; }
}
