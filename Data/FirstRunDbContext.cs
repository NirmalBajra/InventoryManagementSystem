using System;
using InventoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data;

public class FirstRunDbContext: DbContext
{
    public FirstRunDbContext(DbContextOptions<FirstRunDbContext> options): base(options)
    {

    }
    public DbSet<Product> Products { get; set;}
    public DbSet<ProductCategory> ProductCategories { get; set;}
    public DbSet<Purchase> Purchases { get; set;}
    public DbSet<PurchaseDetails> PurchaseDetails { get; set;}
    public DbSet<Sales> Sales { get; set;}
    public DbSet<SalesDetails> SalesDetails { get; set;}
    public DbSet<Stock> Stocks { get; set;}
    public DbSet<StockFlow> StockFlows { get; set;}
    public DbSet<Supplier> Suppliers { get; set;}
    public DbSet<User> Users { get; set;}
}
