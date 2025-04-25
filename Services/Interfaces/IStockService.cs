using System;
using InventoryManagementSystem.Entity;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IStockService
{
    Task UpdateStockOnPurchase(PurchaseDetails details);
    Task<List<Stock>> GetStocksForProduct(int id);
    Task<bool> DeductStock(int productId, int quantity, string updatedBy);
    Task RestoreStock(int productId, int quantity, string updatedBy);

}
