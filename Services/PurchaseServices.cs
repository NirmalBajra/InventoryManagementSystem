using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Dto;
using InventoryManagementSystem.Dtos.PurchaseDtos;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Purchase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class PurchaseServices : IPurchaseService
{
    private readonly FirstRunDbContext dbContext;
    private readonly IStockService stockService;
    public PurchaseServices(FirstRunDbContext dbContext, IStockService stockService)
    {
        this.dbContext = dbContext;
        this.stockService = stockService;
    }

    public async Task AddPurchaseAsync(PurchaseDto dto)
    {
        var purchase = new Purchase
        {
            SupplierId = dto.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            TotalAmount = dto.PurchaseDetails.Sum(d => d.Quantity * d.UnitPrice),
            PurchaseDetails = new List<PurchaseDetails>()
        };

        foreach (var detail in dto.PurchaseDetails)
        {
            // Create purchase detail entry
            var purchaseDetail = new PurchaseDetails
            {
                ProductId = detail.ProductId,
                CategoryId = detail.CategoryId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice
            };

            purchase.PurchaseDetails.Add(purchaseDetail);

            // Check if stock for product with same price exists
            var existingStock = await dbContext.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == detail.ProductId && s.UnitPrice == detail.UnitPrice);

            if (existingStock != null)
            {
                // Update stock
                existingStock.Quantity += detail.Quantity;
                existingStock.AvailableQuantity += detail.Quantity;
            }
            else
            {
                // Create new stock
                existingStock = new Stock
                {
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    AvailableQuantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Stocks.Add(existingStock);
                await dbContext.SaveChangesAsync(); // Ensure StockId is generated
            }

            // Create stock flow
            var stockFlow = new StockFlow
            {
                StockId = existingStock.StockId,
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                TotalCost = detail.Quantity * detail.UnitPrice,
                StockType = Enums.StockType.In, // Assuming "In" means addition
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "System" // Replace with logged-in username if available
            };

            dbContext.StockFlows.Add(stockFlow);
        }

        dbContext.Purchases.Add(purchase);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<PurchaseListDto>> GetAllPurchasesAsync()
    {
        return await dbContext.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseDetails)
            .Select(p => new PurchaseListDto
            {
                Id = p.PurchaseId,
                SupplierName = p.Supplier.SupplierName,
                PurchaseDate = p.PurchaseDate,
                TotalAmount = p.PurchaseDetails.Sum(d => d.Quantity * d.UnitPrice)
            })
            .ToListAsync();
    }

    public async Task<Purchase> GetPurchaseByIdAsync(int id)
    {
        var purchase = await dbContext.Purchases
        .Include(p => p.Supplier)
        .Include(p => p.PurchaseDetails)
            .ThenInclude(pd => pd.Product)
                .ThenInclude(p => p.Category)
        .FirstOrDefaultAsync(p => p.PurchaseId == id);

        if (purchase == null)
        {
            throw new UserFriendlyException($"Purchase with ID {id} not found.");
        }

        return purchase;
    }

    public async Task UpdatePurchaseAsync(int purchaseId, PurchaseDto dto)
    {
        var purchase = await dbContext.Purchases
            .Include(p => p.PurchaseDetails)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

        if (purchase == null) throw new Exception("Purchase not found");

        // Remove previous stock entries and purchase details
        foreach (var detail in purchase.PurchaseDetails)
        {
            var stock = await dbContext.Stocks.FirstOrDefaultAsync(s =>
                s.ProductId == detail.ProductId &&
                s.UnitPrice == detail.UnitPrice &&
                s.AvailableQuantity >= detail.Quantity);

            if (stock != null)
            {
                stock.Quantity -= detail.Quantity;
                stock.AvailableQuantity -= detail.Quantity;
            }

            var flow = await dbContext.StockFlows
                .Where(sf => sf.ProductId == detail.ProductId &&
                             sf.UnitPrice == detail.UnitPrice &&
                             sf.Quantity == detail.Quantity &&
                             sf.StockType == Enums.StockType.In)
                .FirstOrDefaultAsync();

            if (flow != null)
                dbContext.StockFlows.Remove(flow);
        }

        dbContext.PurchaseDetails.RemoveRange(purchase.PurchaseDetails);

        // Update purchase info
        purchase.SupplierId = dto.SupplierId;
        purchase.PurchaseDate = DateTime.UtcNow;
        purchase.TotalAmount = dto.PurchaseDetails.Sum(d => d.Quantity * d.UnitPrice);
        purchase.PurchaseDetails = dto.PurchaseDetails.Select(d =>
                {
                var product = dbContext.Products.FirstOrDefault(p => p.ProductId == d.ProductId);
                if (product == null) throw new Exception($"Product with ID {d.ProductId} not found.");

                return new PurchaseDetails
                {
                    ProductId = d.ProductId,
                    CategoryId = product.CategoryId,  // Use the category from Product entity
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                };
                }).ToList();

        // Add new stock entries and stock flow
        foreach (var detail in purchase.PurchaseDetails)
        {
            var stock = await dbContext.Stocks.FirstOrDefaultAsync(s =>
                s.ProductId == detail.ProductId &&
                s.UnitPrice == detail.UnitPrice);

            if (stock != null)
            {
                stock.Quantity += detail.Quantity;
                stock.AvailableQuantity += detail.Quantity;
            }
            else
            {
                stock = new Stock
                {
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    AvailableQuantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Stocks.Add(stock);
            }

            var stockFlow = new StockFlow
            {
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                TotalCost = detail.Quantity * detail.UnitPrice,
                CreatedAt = DateTime.UtcNow,
                StockType = Enums.StockType.In,
                UpdatedBy = "System"
            };
            dbContext.StockFlows.Add(stockFlow);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task DeletePurchaseAsync(int purchaseId)
    {
        var purchase = await dbContext.Purchases
            .Include(p => p.PurchaseDetails)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

        if (purchase == null) throw new Exception("Purchase not found");

        foreach (var detail in purchase.PurchaseDetails)
        {
            var stock = await dbContext.Stocks.FirstOrDefaultAsync(s =>
                s.ProductId == detail.ProductId &&
                s.UnitPrice == detail.UnitPrice);

            if (stock != null)
            {
                stock.Quantity -= detail.Quantity;
                stock.AvailableQuantity -= detail.Quantity;
                if (stock.Quantity <= 0) dbContext.Stocks.Remove(stock);
            }

            var flow = await dbContext.StockFlows
                .Where(sf => sf.ProductId == detail.ProductId &&
                             sf.UnitPrice == detail.UnitPrice &&
                             sf.Quantity == detail.Quantity &&
                             sf.StockType == Enums.StockType.In)
                .FirstOrDefaultAsync();

            if (flow != null)
                dbContext.StockFlows.Remove(flow);
        }

        dbContext.PurchaseDetails.RemoveRange(purchase.PurchaseDetails);
        dbContext.Purchases.Remove(purchase);

        await dbContext.SaveChangesAsync();
    }

}
