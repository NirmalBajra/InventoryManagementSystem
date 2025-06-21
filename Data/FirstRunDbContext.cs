using System;
using InventoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data;

public class FirstRunDbContext : DbContext
{
    public FirstRunDbContext(DbContextOptions<FirstRunDbContext> options) : base(options)
    {

    }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseDetails> PurchaseDetails { get; set; }
    public DbSet<Sales> Sales { get; set; }
    public DbSet<SalesDetails> SalesDetails { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<StockFlow> StockFlows { get; set; }
    public DbSet<StockAlert> StockAlerts { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<User> Users { get; set; }

    // 👉 Add this method here:
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<Stock>()
            .HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId);

        modelBuilder.Entity<StockFlow>()
            .HasOne(sf => sf.Product)
            .WithMany()
            .HasForeignKey(sf => sf.ProductId);

        modelBuilder.Entity<StockAlert>()
            .HasOne(sa => sa.Product)
            .WithMany()
            .HasForeignKey(sa => sa.ProductId);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Sales)
            .WithMany()
            .HasForeignKey(i => i.SalesId);

        // Configure string properties to not be nullable
        modelBuilder.Entity<User>()
            .Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<StockFlow>()
            .Property(sf => sf.UpdatedBy)
            .IsRequired()
            .HasMaxLength(100);
    }
}
