using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Entity;

public class SalesDetails
{
    [Key]
    public int SalesDetailsId { get; set;}
    public int SalesId { get; set;}
    public int ProductId { get; set;}
    public int Quantity { get; set;}
    public decimal UnitPrice { get; set;}
    public decimal TotalPrice { get; set;}

    public Sales Sales { get; set;}
    public Product Product { get; set;}
}
