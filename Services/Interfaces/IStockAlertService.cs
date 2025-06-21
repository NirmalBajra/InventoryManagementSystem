using InventoryManagementSystem.Entity;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IStockAlertService
{
    Task<List<StockAlert>> GetUnreadAlertsAsync();
    Task<List<StockAlert>> GetAllAlertsAsync();
    Task MarkAlertAsReadAsync(int alertId);
    Task MarkAllAlertsAsReadAsync();
    Task CreateLowStockAlertAsync(int productId, int currentQuantity, int threshold);
    Task CreateOutOfStockAlertAsync(int productId);
    Task CheckAndCreateStockAlertsAsync();
    Task<int> GetUnreadAlertCountAsync();
} 