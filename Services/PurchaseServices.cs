using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Purchase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class PurchaseServices : IPurchaseService
{
    private readonly FirstRunDbContext dbContext;
    private readonly IStockService stockService;
    public PurchaseServices(FirstRunDbContext dbContext, IStockService stockService)
    {
        this.dbContext = dbContext;
        this.stockService = stockService;
    }

    //Purchase made
    public async Task<PurchaseVm> AddPurchase(PurchaseVm vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var purchase = new Purchase
        {
            SupplierId = vm.SupplierId,
            CreatedAt = vm.CreatedAt != default ? vm.CreatedAt : DateTime.UtcNow,
            PurchaseDetails = new List<PurchaseDetails>()
        };
        dbContext.Purchases.Add(purchase);
        await dbContext.SaveChangesAsync();

        foreach (var d in vm.PurchaseDetails)
        {
            var detail = new PurchaseDetails
            {
                PurchaseId = purchase.PurchaseId,
                ProductId = d.ProductId,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                TotalPrice = d.UnitPrice * d.Quantity,
            };
            purchase.PurchaseDetails.Add(detail);

            await stockService.UpdateStockOnPurchase(detail);
        }

        //Set TotalAmount after all details added
        purchase.TotalAmount = purchase.PurchaseDetails.Sum(d => d.TotalPrice);

        dbContext.Purchases.Update(purchase);

        await dbContext.SaveChangesAsync();
        tnx.Complete();
        return vm;
    }

    // Get all purchases 
    [HttpGet]
    public async Task<List<PurchaseListVm>> GetAllPurchases()
    {
        var purchases = await dbContext.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseDetails)
                .ThenInclude(d => d.Product)    
            .ToListAsync();

        return purchases.Select(p => new PurchaseListVm
        {
            PurchaseId = p.PurchaseId,
            SupplierId = p.SupplierId,
            SupplierName = p.Supplier.SupplierName,
            CreatedAt = p.CreatedAt,
            TotalAmount = p.TotalAmount,
            PurchaseDetails = p.PurchaseDetails.Select(d => new PurchaseDetailVm
            {
                ProductId = d.ProductId,
                ProductName = d.Product.ProductName,
                CategoryName = d.Product.Category.CategoryName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice
            }).ToList()
        }).ToList();
    }

    // Get Purchases By ID
    public async Task<Purchase?> GetPurchaseById(int id)
    {
        return await dbContext.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseDetails).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);
    }

    public async Task<bool> UpdatePurchase(int id, PurchaseVm vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var existingPurchase = await dbContext.Purchases
                        .Include(p => p.PurchaseDetails)
                        .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (existingPurchase == null) return false;

            existingPurchase.SupplierId = vm.SupplierId;
            existingPurchase.CreatedAt = vm.CreatedAt;

            // 1. Rollback old stock changes
            foreach (var oldDetail in existingPurchase.PurchaseDetails)
            {
                var stock = await dbContext.Stocks
                    .FirstOrDefaultAsync(s => s.ProductId == oldDetail.ProductId && s.UnitPrice == oldDetail.UnitPrice);

                if (stock != null)
                {
                    stock.Quantity -= oldDetail.Quantity;
                    stock.AvailableQuantity -= oldDetail.Quantity;

                    if (stock.Quantity <= 0)
                    {
                        dbContext.Stocks.Remove(stock);
                    }
                    else
                    {
                        dbContext.Stocks.Update(stock);
                    }
                }

                // Delete old StockFlows related to this purchase
                var stockFlows = await dbContext.StockFlows
                    .Where(sf => sf.ProductId == oldDetail.ProductId
                                 && sf.UnitPrice == oldDetail.UnitPrice
                                 && sf.StockType == Enums.StockType.In) // Only IN flows
                    .ToListAsync();

                dbContext.StockFlows.RemoveRange(stockFlows);
            }

            await dbContext.SaveChangesAsync();

            // 2. Remove old PurchaseDetails
            dbContext.PurchaseDetails.RemoveRange(existingPurchase.PurchaseDetails);
            await dbContext.SaveChangesAsync();

            // 3. Add new PurchaseDetails
            existingPurchase.PurchaseDetails = vm.PurchaseDetails.Select(d => new PurchaseDetails
            {
                ProductId = d.ProductId,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                TotalPrice = d.UnitPrice * d.Quantity,
                PurchaseId = id
            }).ToList();

            existingPurchase.TotalAmount = existingPurchase.PurchaseDetails.Sum(d => d.TotalPrice);

            dbContext.Purchases.Update(existingPurchase);
            await dbContext.SaveChangesAsync();

            // 4. Update stock with new purchase details
            foreach (var detail in existingPurchase.PurchaseDetails)
            {
                await stockService.UpdateStockOnPurchase(detail);
            }

            tnx.Complete();
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
