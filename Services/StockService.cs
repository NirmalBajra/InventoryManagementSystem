using System;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Dtos.StockDtos;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class StockService : IStockService
{
    private readonly FirstRunDbContext dbContext;
    private readonly IStockAlertService stockAlertService;
    private const int DEFAULT_LOW_STOCK_THRESHOLD = 10;

    public StockService(FirstRunDbContext dbContext, IStockAlertService stockAlertService)
    {
        this.dbContext = dbContext;
        this.stockAlertService = stockAlertService;
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
                Quantity = deductQty,
                UnitPrice = stock.UnitPrice,
                TotalCost = stock.UnitPrice * deductQty,
                StockType = Enums.StockType.Out,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            });

            // Check for low stock alerts after deduction
            if (stock.AvailableQuantity <= DEFAULT_LOW_STOCK_THRESHOLD)
            {
                await stockAlertService.CreateLowStockAlertAsync(stock.ProductId, stock.AvailableQuantity, DEFAULT_LOW_STOCK_THRESHOLD);
            }

            // Check for out of stock alert
            if (stock.AvailableQuantity == 0)
            {
                await stockAlertService.CreateOutOfStockAlertAsync(stock.ProductId);
            }
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

    public async Task<List<ViewStockDto>> ViewStockAsync()
    {
        try
        {
            var stockList = await dbContext.Stocks
                .Include(s => s.Product)
                .ThenInclude(p => p.Category)
                .Select(s => new ViewStockDto
                {
                    ProductId = s.ProductId,
                    ProductName = s.Product.ProductName,
                    CategoryName = s.Product.Category.CategoryName,
                    AvailableQuantity = s.AvailableQuantity,
                    UnitPrice = s.UnitPrice,
                    ImagePath = s.Product.ImagePath,
                    Quantity = s.Quantity
                }).ToListAsync();
            return stockList;
        }
        catch (Exception ex)
        {
            throw new UserNotFoundException("Stock Not found." + ex.Message);
        }
    }

    // New methods for enhanced stock management

    public async Task<List<ViewStockDto>> GetLowStockItemsAsync()
    {
        return await dbContext.Stocks
            .Include(s => s.Product)
            .ThenInclude(p => p.Category)
            .Where(s => s.AvailableQuantity <= DEFAULT_LOW_STOCK_THRESHOLD)
            .Select(s => new ViewStockDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product.ProductName,
                CategoryName = s.Product.Category.CategoryName,
                AvailableQuantity = s.AvailableQuantity,
                UnitPrice = s.UnitPrice,
                ImagePath = s.Product.ImagePath,
                Quantity = s.Quantity
            }).ToListAsync();
    }

    public async Task<List<ViewStockDto>> GetOutOfStockItemsAsync()
    {
        return await dbContext.Stocks
            .Include(s => s.Product)
            .ThenInclude(p => p.Category)
            .Where(s => s.AvailableQuantity == 0)
            .Select(s => new ViewStockDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product.ProductName,
                CategoryName = s.Product.Category.CategoryName,
                AvailableQuantity = s.AvailableQuantity,
                UnitPrice = s.UnitPrice,
                ImagePath = s.Product.ImagePath,
                Quantity = s.Quantity
            }).ToListAsync();
    }

    public async Task<StockAnalysisResult> AnalyzeStockMovementAsync(int productId, DateTime startDate, DateTime endDate)
    {
        var stockFlows = await dbContext.StockFlows
            .Where(sf => sf.ProductId == productId && 
                        sf.CreatedAt >= startDate && 
                        sf.CreatedAt <= endDate)
            .ToListAsync();

        var stockIn = stockFlows.Where(sf => sf.StockType == Enums.StockType.In).Sum(sf => sf.Quantity);
        var stockOut = stockFlows.Where(sf => sf.StockType == Enums.StockType.Out).Sum(sf => sf.Quantity);
        var totalCost = stockFlows.Where(sf => sf.StockType == Enums.StockType.In).Sum(sf => sf.TotalCost);

        var currentStock = await dbContext.Stocks
            .Where(s => s.ProductId == productId)
            .SumAsync(s => s.AvailableQuantity);

        return new StockAnalysisResult
        {
            ProductId = productId,
            StockIn = stockIn,
            StockOut = stockOut,
            NetMovement = stockIn - stockOut,
            CurrentStock = currentStock,
            TotalCost = totalCost,
            AverageCost = stockIn > 0 ? totalCost / stockIn : 0,
            TurnoverRate = stockIn > 0 ? (double)stockOut / stockIn : 0
        };
    }

    public async Task<List<StockFlow>> GetStockFlowHistoryAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = dbContext.StockFlows
            .Include(sf => sf.Product)
            .Where(sf => sf.ProductId == productId);

        if (startDate.HasValue)
            query = query.Where(sf => sf.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(sf => sf.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(sf => sf.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageStockValueAsync()
    {
        return await dbContext.Stocks
            .SumAsync(s => s.AvailableQuantity * s.UnitPrice);
    }

    public async Task<Dictionary<string, decimal>> GetStockValueByCategoryAsync()
    {
        return await dbContext.Stocks
            .Include(s => s.Product)
            .ThenInclude(p => p.Category)
            .GroupBy(s => s.Product.Category.CategoryName)
            .Select(g => new { Category = g.Key, Value = g.Sum(s => s.AvailableQuantity * s.UnitPrice) })
            .ToDictionaryAsync(x => x.Category, x => x.Value);
    }
}

// New class for stock analysis results
public class StockAnalysisResult
{
    public int ProductId { get; set; }
    public int StockIn { get; set; }
    public int StockOut { get; set; }
    public int NetMovement { get; set; }
    public int CurrentStock { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCost { get; set; }
    public double TurnoverRate { get; set; }
}
