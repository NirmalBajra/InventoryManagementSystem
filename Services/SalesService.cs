using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Dtos.SalesDtos;
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
    private readonly IStockAlertService stockAlertService;
    private readonly IInvoiceService invoiceService;

    public SalesService(FirstRunDbContext dbContext, IStockService stockService, IStockAlertService stockAlertService, IInvoiceService invoiceService)
    {
        this.dbContext = dbContext;
        this.stockService = stockService;
        this.stockAlertService = stockAlertService;
        this.invoiceService = invoiceService;
    }

    public async Task CreateSalesAsync(CreateSalesDto input, string updatedBy)
    {
        // Begin a transaction to ensure all-or-nothing for sale and stock updates
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            // Validate input
            if (input.SalesDetails == null || !input.SalesDetails.Any())
            {
                throw new ArgumentException("Sales details cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(input.CustomerName))
            {
                throw new ArgumentException("Customer name is required.");
            }

            decimal totalAmount = 0;

            // Create the Sales entity
            var salesDate = input.SalesDate == default ? DateTime.UtcNow : (input.SalesDate.Kind == DateTimeKind.Utc ? input.SalesDate : input.SalesDate.ToUniversalTime());
            var sales = new Sales
            {
                CustomerName = input.CustomerName,
                SalesDate = salesDate,
                CreateAt = DateTime.UtcNow,
                SalesDetails = new List<SalesDetails>()
            };

            // For each product in the sale, deduct stock using FIFO
            foreach (var item in input.SalesDetails)
            {
                // Validate item
                if (item.Quantity <= 0)
                {
                    throw new ArgumentException($"Invalid quantity for product ID {item.ProductId}.");
                }

                if (item.UnitPrice <= 0)
                {
                    throw new ArgumentException($"Invalid unit price for product ID {item.ProductId}.");
                }

                int remainingQty = item.Quantity;

                // Get all available stock batches for the product, ordered by CreatedAt (FIFO)
                var stocks = await dbContext.Stocks
                    .Where(s => s.ProductId == item.ProductId && s.AvailableQuantity > 0)
                    .OrderBy(s => s.CreatedAt)
                    .ToListAsync();

                if (!stocks.Any())
                {
                    throw new InvalidOperationException($"No stock available for Product ID {item.ProductId}");
                }

                // Deduct from each stock batch until the sale quantity is fulfilled
                foreach (var stock in stocks)
                {
                    if (remainingQty <= 0)
                        break;

                    int usedQty = Math.Min(stock.AvailableQuantity, remainingQty);
                    stock.AvailableQuantity -= usedQty;
                    remainingQty -= usedQty;

                    // Record the stock outflow in StockFlow
                    var stockFlow = new StockFlow
                    {
                        StockId = stock.StockId,
                        ProductId = item.ProductId,
                        Quantity = usedQty,
                        UnitPrice = stock.UnitPrice,
                        TotalCost = usedQty * stock.UnitPrice,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = updatedBy,
                        StockType = Enums.StockType.Out
                    };
                    await dbContext.StockFlows.AddAsync(stockFlow);

                    // Trigger low stock alert if below threshold
                    if (stock.AvailableQuantity <= 10)
                    {
                        await stockAlertService.CreateLowStockAlertAsync(stock.ProductId, stock.AvailableQuantity, 10);
                    }

                    // Trigger out of stock alert if depleted
                    if (stock.AvailableQuantity == 0)
                    {
                        await stockAlertService.CreateOutOfStockAlertAsync(stock.ProductId);
                    }
                }
                
                // If not enough stock was available, throw an error
                if (remainingQty > 0)
                {
                    throw new InvalidOperationException($"Not enough stock available for Product Id {item.ProductId}. Required: {item.Quantity}, Available: {item.Quantity - remainingQty}");
                }

                // Add the sales detail to the sale
                var SalesDetail = new SalesDetails
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                };
                sales.SalesDetails.Add(SalesDetail);
                totalAmount += SalesDetail.TotalPrice;
            }
            sales.TotalAmount = totalAmount;

            // Ensure all DateTimes are UTC before saving
            sales.SalesDate = sales.SalesDate.Kind == DateTimeKind.Utc
                ? sales.SalesDate
                : (sales.SalesDate.Kind == DateTimeKind.Local
                    ? sales.SalesDate.ToUniversalTime()
                    : DateTime.SpecifyKind(sales.SalesDate, DateTimeKind.Utc));
            sales.CreateAt = DateTime.UtcNow;
            // Debug log
            Console.WriteLine($"[DEBUG] SalesDate: {sales.SalesDate} Kind: {sales.SalesDate.Kind}");
            Console.WriteLine($"[DEBUG] CreateAt: {sales.CreateAt} Kind: {sales.CreateAt.Kind}");
            foreach (var detail in sales.SalesDetails)
            {
                // If you add any DateTime to SalesDetails in the future, ensure UTC here
                // Console.WriteLine($"[DEBUG] SalesDetails DateTime: ...");
            }

            // Save the sale
            await dbContext.Sales.AddAsync(sales);
            await dbContext.SaveChangesAsync();
            
            // Generate invoice automatically after sale
            try
            {
                await invoiceService.GenerateInvoiceAsync(sales.SalesId);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the sale
                Console.WriteLine($"Error generating invoice: {ex.Message}");
            }
            
            tnx.Complete();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"An error occurred while creating the sale. Details: {ex.Message}");
        }
    }

    public async Task DeleteSalesAsync(int id)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            var sale = await dbContext.Sales
                .Include(s => s.SalesDetails)
                .FirstOrDefaultAsync(s => s.SalesId == id);
                
            if (sale == null)
            {
                throw new UserFriendlyException("Sale not found.");
            }

            foreach (var detail in sale.SalesDetails)
            {
                var relatedStockFlow = await dbContext.StockFlows
                    .Where(sf => sf.ProductId == detail.ProductId && sf.StockType == Enums.StockType.Out)
                    .OrderByDescending(sf => sf.CreatedAt)
                    .ToListAsync();

                int remainingQty = detail.Quantity;

                foreach (var flow in relatedStockFlow)
                {
                    if (remainingQty <= 0) break;

                    var stock = await dbContext.Stocks.FindAsync(flow.StockId);
                    if (stock == null) continue;

                    int restoreQty = Math.Min(flow.Quantity, remainingQty);
                    stock.AvailableQuantity += restoreQty;

                    if (restoreQty == flow.Quantity)
                    {
                        dbContext.StockFlows.Remove(flow);
                    }
                    else
                    {
                        flow.Quantity -= restoreQty;
                        flow.TotalCost = flow.UnitPrice * flow.Quantity;
                        dbContext.StockFlows.Update(flow);
                    }

                    remainingQty -= restoreQty;
                }
                
                if (remainingQty > 0)
                {
                    throw new InvalidOperationException($"Unable to fully restore stock for Product ID {detail.ProductId}");
                }
            }
            
            dbContext.Sales.Remove(sale);
            await dbContext.SaveChangesAsync();
            tnx.Complete();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"An error occurred while deleting the sale. Details: {ex.Message}");
        }
    }

    public async Task<(List<Sales> sales, int totalRecords)> GetAllSalesAsync(SalesFilterDto filter)
    {
        var query = dbContext.Sales
            .Include(s => s.SalesDetails)
            .ThenInclude(sd => sd.Product)
            .AsQueryable();

        if (filter.DateFrom.HasValue)
        {
            var dateFrom = DateTime.SpecifyKind(filter.DateFrom.Value.Date, DateTimeKind.Utc);
            query = query.Where(s => s.SalesDate >= dateFrom);
        }

        if (filter.DateTo.HasValue)
        {
            var dateTo = DateTime.SpecifyKind(filter.DateTo.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(s => s.SalesDate < dateTo);
        }

        var totalRecords = await query.CountAsync();

        var sales = await query
            .OrderByDescending(s => s.SalesDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (sales, totalRecords);
    }

    public async Task<Sales> GetSalesByIdAsync(int id)
    {
        return await dbContext.Sales
            .Include(s => s.SalesDetails)
            .ThenInclude(sd => sd.Product)
            .FirstOrDefaultAsync(s => s.SalesId == id);
    }

    public async Task UpdateSalesAsync(int id, CreateSalesDto input, string updatedBy)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            var existingSale = await dbContext.Sales
                .Include(s => s.SalesDetails)
                .FirstOrDefaultAsync(s => s.SalesId == id);

            if (existingSale == null)
            {
                throw new UserFriendlyException("Sale not found.");
            }

            // Restore stock from existing sale
            foreach (var detail in existingSale.SalesDetails)
            {
                await stockService.RestoreStock(detail.ProductId, detail.Quantity, updatedBy);
            }

            // Remove existing details
            dbContext.SalesDetails.RemoveRange(existingSale.SalesDetails);

            // Update the existing sale
            existingSale.CustomerName = input.CustomerName;
            existingSale.SalesDate = input.SalesDate;

            // Process new sales details
            decimal totalAmount = 0;
            var newSalesDetails = new List<SalesDetails>();

            foreach (var item in input.SalesDetails)
            {
                // Validate item
                if (item.Quantity <= 0)
                {
                    throw new ArgumentException($"Invalid quantity for product ID {item.ProductId}.");
                }

                if (item.UnitPrice <= 0)
                {
                    throw new ArgumentException($"Invalid unit price for product ID {item.ProductId}.");
                }

                int remainingQty = item.Quantity;

                // Get all available stock batches for the product, ordered by CreatedAt (FIFO)
                var stocks = await dbContext.Stocks
                    .Where(s => s.ProductId == item.ProductId && s.AvailableQuantity > 0)
                    .OrderBy(s => s.CreatedAt)
                    .ToListAsync();

                if (!stocks.Any())
                {
                    throw new InvalidOperationException($"No stock available for Product ID {item.ProductId}");
                }

                // Deduct from each stock batch until the sale quantity is fulfilled
                foreach (var stock in stocks)
                {
                    if (remainingQty <= 0)
                        break;

                    int usedQty = Math.Min(stock.AvailableQuantity, remainingQty);
                    stock.AvailableQuantity -= usedQty;
                    remainingQty -= usedQty;

                    // Record the stock outflow in StockFlow
                    var stockFlow = new StockFlow
                    {
                        StockId = stock.StockId,
                        ProductId = item.ProductId,
                        Quantity = usedQty,
                        UnitPrice = stock.UnitPrice,
                        TotalCost = usedQty * stock.UnitPrice,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = updatedBy,
                        StockType = Enums.StockType.Out
                    };
                    await dbContext.StockFlows.AddAsync(stockFlow);

                    // Trigger low stock alert if below threshold
                    if (stock.AvailableQuantity <= 10)
                    {
                        await stockAlertService.CreateLowStockAlertAsync(stock.ProductId, stock.AvailableQuantity, 10);
                    }

                    // Trigger out of stock alert if depleted
                    if (stock.AvailableQuantity == 0)
                    {
                        await stockAlertService.CreateOutOfStockAlertAsync(stock.ProductId);
                    }
                }
                
                // If not enough stock was available, throw an error
                if (remainingQty > 0)
                {
                    throw new InvalidOperationException($"Not enough stock available for Product Id {item.ProductId}. Required: {item.Quantity}, Available: {item.Quantity - remainingQty}");
                }

                // Add the sales detail to the sale
                var salesDetail = new SalesDetails
                {
                    SalesId = existingSale.SalesId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                };
                newSalesDetails.Add(salesDetail);
                totalAmount += salesDetail.TotalPrice;
            }

            existingSale.TotalAmount = totalAmount;
            existingSale.SalesDetails = newSalesDetails;

            await dbContext.SaveChangesAsync();
            tnx.Complete();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"Error updating sale: {ex.Message}");
        }
    }

    public async Task<List<Sales>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await dbContext.Sales
            .Where(s => s.SalesDate >= startDate && s.SalesDate <= endDate)
            .Include(s => s.SalesDetails)
            .ThenInclude(sd => sd.Product)
            .ToListAsync();
    }

    public async Task<CreateSalesDto> GetSaleForEditAsync(int id)
    {
        var sale = await dbContext.Sales
            .Include(s => s.SalesDetails)
            .FirstOrDefaultAsync(s => s.SalesId == id);

        if (sale == null)
        {
            return null;
        }

        return new CreateSalesDto
        {
            CustomerName = sale.CustomerName,
            SalesDate = sale.SalesDate,
            SalesDetails = sale.SalesDetails.Select(sd => new SalesDetailDto
            {
                ProductId = sd.ProductId,
                Quantity = sd.Quantity,
                UnitPrice = sd.UnitPrice
            }).ToList()
        };
    }

    public async Task<List<Sales>> GetAllSalesAsync()
    {
        return await dbContext.Sales
            .Include(s => s.SalesDetails)
            .ThenInclude(sd => sd.Product)
            .OrderByDescending(s => s.SalesDate)
            .ToListAsync();
    }

    public async Task DeleteSalesAsync(int id, string user)
    {
        var sales = await dbContext.Sales.FindAsync(id);
        if (sales != null)
        {
            dbContext.Sales.Remove(sales);
            await dbContext.SaveChangesAsync();
        }
    }
}
