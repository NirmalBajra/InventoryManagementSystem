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

}
