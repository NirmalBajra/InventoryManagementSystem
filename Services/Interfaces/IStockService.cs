using System;
using InventoryManagementSystem.Dtos.StockDtos;
using InventoryManagementSystem.Entity;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IStockService
{
    Task<List<Stock>> GetStocksForProduct(int id);
    Task<bool> DeductStock(int productId, int quantity, string updatedBy);
    Task RestoreStock(int productId, int quantity, string updatedBy);
    Task<List<ViewStockDto>> ViewStockAsync();
    
    // Enhanced stock management methods
    Task<List<ViewStockDto>> GetLowStockItemsAsync();
    Task<List<ViewStockDto>> GetOutOfStockItemsAsync();
    Task<StockAnalysisResult> AnalyzeStockMovementAsync(int productId, DateTime startDate, DateTime endDate);
    Task<List<StockFlow>> GetStockFlowHistoryAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetAverageStockValueAsync();
    Task<Dictionary<string, decimal>> GetStockValueByCategoryAsync();
}
