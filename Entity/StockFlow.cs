using System;
using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Enums;

namespace InventoryManagementSystem.Entity;

public class StockFlow
{
    [Key]
    public int StockId { get; set;}
    public int ProductId { get; set;}
    public int Quantity { get; set;}
    public StockType StockType { get; set;}
    public decimal UnitPrice { get; set;}
    public decimal TotalCost { get; set;}
    public DateTime CreatedAt { get; set;}
    public string UpdatedBy { get; set;}

    //Navigation
    public Product Product { get; set;}
}