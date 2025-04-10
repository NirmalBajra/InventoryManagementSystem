using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class Purchase
{
    [Key]
    public int PurchaseId { get; set;}
    public int ProductId { get; set;}
    public int SupplierId { get; set;}
    public DateTime CreatedAt { get; set;} = DateTime.UtcNow;

    //Navigation Properties
    public Product Product { get; set;}
    public Supplier Supplier { get; set;}
    public ICollection<PurchaseDetails> PurchaseDetails { get; set;}
}
