using System;
using InventoryManagementSystem.Dto;
using InventoryManagementSystem.Dtos.PurchaseDtos;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.ViewModels.Purchase;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IPurchaseService
{
    Task AddPurchaseAsync(PurchaseDto dto);
    Task<List<PurchaseListDto>> GetAllPurchasesAsync();
    Task<Purchase> GetPurchaseByIdAsync(int id);
}
