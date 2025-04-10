using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class Sales
{
    [Key]
    public int SalesId { get; set;}
    public string CustomerName { get; set;}
    public decimal TotalAmount { get; set;}
    public DateTime SalesDate { get; set;}
    public DateTime CreateAt { get; set;}

    public ICollection<SalesDetails> SalesDetails { get; set;}
}
