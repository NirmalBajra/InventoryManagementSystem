using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class Stock
{
    [Key]
    public int StockId { get; set;}
    public int ProductId { get; set;}
    public int Quantity { get; set;}
    public decimal UnitPrice { get; set;}
    public DateTime CreatedAt { get; set;}

    public Product Product { get; set;}
}
