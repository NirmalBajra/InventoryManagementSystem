using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace InventoryManagementSystem.Services;

public class SalesService : ISalesService
{
    private readonly FirstRunDbContext dbContext;
    private readonly IStockService stockService;
    public SalesService(FirstRunDbContext dbContext, IStockService stockService)
    {
        this.dbContext = dbContext;
        this.stockService = stockService;
    }

    public async Task<int> AddSaleProducts(SalesVm vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var sales = new Sales
        {
            CustomerName = vm.CustomerName,
            SalesDate = vm.SalesDate,
            CreateAt = DateTime.UtcNow,
            SalesDetails = new List<SalesDetails>()
        };

        decimal totalAmount = 0;

        foreach (var item in vm.SalesDetails)
        {
            var success = await stockService.DeductStock(item.ProductId, item.Quantity, "System");
            if (!success)
            {
                throw new UserNotFoundException($"Insufficient stock for ProductId: {item.ProductId}");
            }

            var detail = new SalesDetails
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.UnitPrice * item.Quantity
            };

            sales.SalesDetails.Add(detail);
            totalAmount += detail.TotalPrice;
        }
        sales.TotalAmount = totalAmount;

        dbContext.Sales.Add(sales);
        await dbContext.SaveChangesAsync();
        tnx.Complete();
        return sales.SalesId;
    }

    //Update sales
    public async Task<bool> UpdateSales(int id, SalesVm vm)
    {
        var existingSale = await dbContext.Sales
            .Include(s => s.SalesDetails)
            .FirstOrDefaultAsync(s => s.SalesId == id);

        if (existingSale == null) { return false; }

        foreach (var detail in existingSale.SalesDetails)
        {
            await stockService.RestoreStock(detail.ProductId, detail.Quantity, "System");
        }

        //Remove old Stock
        dbContext.SalesDetails.RemoveRange(existingSale.SalesDetails);

        //update new info
        existingSale.CustomerName = vm.CustomerName;
        existingSale.SalesDate = vm.SalesDate;
        existingSale.CreateAt = DateTime.UtcNow;

        decimal totalAmount = 0;
        existingSale.SalesDetails = new List<SalesDetails>();

        foreach (var item in vm.SalesDetails)
        {
            var success = await stockService.DeductStock(item.ProductId, item.Quantity, "System");
            if (!success)
            {
                throw new Exception($"Insufficient stock for ProductId: {item.ProductId}");
            }

            var detail = new SalesDetails
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.UnitPrice * item.Quantity
            };
            existingSale.SalesDetails.Add(detail);
            totalAmount += detail.TotalPrice;
        }
        existingSale.TotalAmount = totalAmount;

        await dbContext.SaveChangesAsync();
        return true;
    }

    //Delete Sales
    public async Task<bool> DeleteSales(int id)
    {
        var sale = await dbContext.Sales
            .Include(s => s.SalesDetails)
            .FirstOrDefaultAsync(s => s.SalesId == id);

        if (sale == null) return false;

        foreach (var detail in sale.SalesDetails)
        {
            await stockService.RestoreStock(detail.ProductId, detail.Quantity, "System");
        }

        dbContext.SalesDetails.RemoveRange(sale.SalesDetails);
        dbContext.Sales.Remove(sale);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
