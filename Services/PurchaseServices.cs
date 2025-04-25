using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Purchase;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class PurchaseServices : IPurchaseService
{
    private readonly FirstRunDbContext dbContext;
    private readonly IStockService stockService;
    public PurchaseServices(FirstRunDbContext dbContext,IStockService stockService)
    {
        this.dbContext = dbContext;
        this.stockService = stockService;
    }

    //Purchase made
    public async Task<int> AddPurchase(PurchaseVm vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var purchase = new Purchase
        {
            SupplierId = vm.SupplierId,
            CreatedAt = vm.CreatedAt,
            PurchaseDetails = vm.PurchaseDetails.Select(d => new PurchaseDetails
            {
                ProductId = d.ProductId,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                TotalPrice = d.UnitPrice * d.Quantity,
                CategoryId = d.CategoryId
            }).ToList()
        };
        purchase.TotalAmount = purchase.PurchaseDetails.Sum(d => d.TotalPrice);

        dbContext.Purchases.Add(purchase);
        await dbContext.SaveChangesAsync(); //Save the Purchase

        foreach(var detail in purchase.PurchaseDetails)
        {
            await stockService.UpdateStockOnPurchase(detail);
        }

        tnx.Complete();
        return purchase.PurchaseId;
    }

    // Get all purchases 
    public async Task<List<Purchase>> GetAllPurchases()
    {
        return await dbContext.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseDetails).ThenInclude(d => d.Product)
            .Include(p => p.PurchaseDetails).ThenInclude(d => d.Category)
            .ToListAsync();
    }

    // Get Purchases By ID
    public async Task<Purchase?> GetPurchaseById(int id)
    {
        return await dbContext.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseDetails).ThenInclude(d => d.Product)
            .Include(p => p.PurchaseDetails).ThenInclude(d => d.Category)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);
    }

    //Update purchases
    public async Task<bool> UpdatePurchase(int id, PurchaseVm vm)
    {
        try
        {
            var existingPurchase = await dbContext.Purchases
                        .Include(p => p.PurchaseDetails)
                        .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (existingPurchase == null) return false;

            existingPurchase.SupplierId = vm.SupplierId;
            existingPurchase.CreatedAt = vm.CreatedAt;

            //Remove old details
            dbContext.PurchaseDetails.RemoveRange(existingPurchase.PurchaseDetails);

            //Add new Details
            existingPurchase.PurchaseDetails = vm.PurchaseDetails.Select(d => new PurchaseDetails
            {
                ProductId = d.ProductId,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                TotalPrice = d.UnitPrice * d.Quantity,
                CategoryId = d.CategoryId,
                PurchaseId = id
            }).ToList();

            existingPurchase.TotalAmount = existingPurchase.PurchaseDetails.Sum(d => d.TotalPrice);

            await dbContext.SaveChangesAsync();

            foreach(var detail in existingPurchase.PurchaseDetails)
            {
                await stockService.UpdateStockOnPurchase(detail);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    //Delete Purchase record
    public async Task<bool> DeletePurchase(int id)
    {
        try
        {
            var purchase = await dbContext.Purchases
            .Include(p => p.PurchaseDetails)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null) return false;

            dbContext.PurchaseDetails.RemoveRange(purchase.PurchaseDetails);
            dbContext.Purchases.Remove(purchase);
            await dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {

            return false;
        }
    }
}
