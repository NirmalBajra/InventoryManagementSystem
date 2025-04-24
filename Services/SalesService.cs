using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Sales;

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
            totalAmount +=detail.TotalPrice;
        }
        sales.TotalAmount = totalAmount;

        dbContext.Sales.Add(sales);
        await dbContext.SaveChangesAsync();
        tnx.Complete();
        return sales.SalesId;
    }

}
