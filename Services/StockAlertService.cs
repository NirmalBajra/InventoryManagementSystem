using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class StockAlertService : IStockAlertService
{
    private readonly FirstRunDbContext _dbContext;
    private const int DEFAULT_LOW_STOCK_THRESHOLD = 10;

    public StockAlertService(FirstRunDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<StockAlert>> GetUnreadAlertsAsync()
    {
        return await _dbContext.StockAlerts
            .Include(sa => sa.Product)
            .Where(sa => !sa.IsRead)
            .OrderByDescending(sa => sa.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<StockAlert>> GetAllAlertsAsync()
    {
        return await _dbContext.StockAlerts
            .Include(sa => sa.Product)
            .OrderByDescending(sa => sa.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAlertAsReadAsync(int alertId)
    {
        var alert = await _dbContext.StockAlerts.FindAsync(alertId);
        if (alert != null)
        {
            alert.IsRead = true;
            alert.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task MarkAllAlertsAsReadAsync()
    {
        var unreadAlerts = await _dbContext.StockAlerts
            .Where(sa => !sa.IsRead)
            .ToListAsync();

        foreach (var alert in unreadAlerts)
        {
            alert.IsRead = true;
            alert.ReadAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateLowStockAlertAsync(int productId, int currentQuantity, int threshold)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null) return;

        var existingAlert = await _dbContext.StockAlerts
            .FirstOrDefaultAsync(sa => sa.ProductId == productId && 
                                     sa.AlertType == "LowStock" && 
                                     !sa.IsRead);

        if (existingAlert == null)
        {
            var alert = new StockAlert
            {
                ProductId = productId,
                AlertType = "LowStock",
                Message = $"Low stock alert: {product.ProductName} has only {currentQuantity} units remaining (threshold: {threshold})",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.StockAlerts.Add(alert);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task CreateOutOfStockAlertAsync(int productId)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null) return;

        var existingAlert = await _dbContext.StockAlerts
            .FirstOrDefaultAsync(sa => sa.ProductId == productId && 
                                     sa.AlertType == "OutOfStock" && 
                                     !sa.IsRead);

        if (existingAlert == null)
        {
            var alert = new StockAlert
            {
                ProductId = productId,
                AlertType = "OutOfStock",
                Message = $"Out of stock alert: {product.ProductName} is completely out of stock",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.StockAlerts.Add(alert);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task CheckAndCreateStockAlertsAsync()
    {
        var stocks = await _dbContext.Stocks
            .Include(s => s.Product)
            .ToListAsync();

        foreach (var stock in stocks)
        {
            if (stock.AvailableQuantity == 0)
            {
                await CreateOutOfStockAlertAsync(stock.ProductId);
            }
            else if (stock.AvailableQuantity <= DEFAULT_LOW_STOCK_THRESHOLD)
            {
                await CreateLowStockAlertAsync(stock.ProductId, stock.AvailableQuantity, DEFAULT_LOW_STOCK_THRESHOLD);
            }
        }
    }

    public async Task<int> GetUnreadAlertCountAsync()
    {
        return await _dbContext.StockAlerts
            .CountAsync(sa => !sa.IsRead);
    }
} 