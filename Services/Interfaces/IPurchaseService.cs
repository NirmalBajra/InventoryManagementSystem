using System;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.ViewModels.Purchase;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IPurchaseService
{
    Task<int> AddPurchase(PurchaseVm vm);
    Task<List<Purchase>> GetAllPurchases();
    Task<Purchase?> GetPurchaseById(int id);
    Task<bool> UpdatePurchase(int id, PurchaseVm vm);
    Task<bool> DeletePurchase(int id);
}
