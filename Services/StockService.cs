using System;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class StockService : IStockService
{
    private readonly FirstRunDbContext dbContext;
    public StockService(FirstRunDbContext dbContext)
    {
        this.dbContext = dbContext;
    }



    // Get Stock for Products
    public async Task<List<Stock>> GetStocksForProduct(int productId)
    {
        return await dbContext.Stocks
            .Where(s => s.ProductId == productId && s.AvailableQuantity > 0)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    //Deduct stock for products
    public async Task<bool> DeductStock(int productId, int quantity, string updatedBy)
    {
        var stocks = await GetStocksForProduct(productId);
        int remaining = quantity;

        foreach (var stock in stocks)
        {
            if (remaining <= 0) break;

            int deductQty = Math.Min(stock.AvailableQuantity, remaining);
            stock.AvailableQuantity -= deductQty;
            remaining -= deductQty;

            dbContext.StockFlows.Add(new StockFlow
            {
                StockId = stock.StockId,
                ProductId = stock.ProductId,
                Quantity = stock.Quantity,
                UnitPrice = stock.UnitPrice,
                TotalCost = stock.UnitPrice * deductQty,
                StockType = Enums.StockType.Out,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            });
        }
        await dbContext.SaveChangesAsync();
        return remaining == 0;
    }

    //RestoreStock
    public async Task RestoreStock(int productId, int quantity, string updatedBy)
    {
        var stocks = await dbContext.Stocks
            .Where(s => s.ProductId == productId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        int remaining = quantity;

        foreach (var stock in stocks)
        {
            if (remaining <= 0) break;

            int restoreQty = Math.Min(stock.Quantity - stock.AvailableQuantity, remaining);
            stock.AvailableQuantity += restoreQty;
            remaining -= restoreQty;

            dbContext.StockFlows.Add(new StockFlow
            {
                StockId = stock.StockId,
                ProductId = stock.ProductId,
                Quantity = restoreQty,
                UnitPrice = stock.UnitPrice,
                TotalCost = stock.UnitPrice * restoreQty,
                StockType = Enums.StockType.In,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            });
        }
        await dbContext.SaveChangesAsync();
    }
}
