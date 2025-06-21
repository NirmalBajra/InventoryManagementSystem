using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

[Authorize]
public class DashboardsController : Controller
{
    private readonly FirstRunDbContext dbContext;
    private readonly ISalesService salesService;
    private readonly IPurchaseService purchaseService;
    private readonly IStockService stockService;
    private readonly IUserServices userServices;

    public DashboardsController(
        FirstRunDbContext dbContext,
        ISalesService salesService,
        IPurchaseService purchaseService,
        IStockService stockService,
        IUserServices userServices)
    {
        this.dbContext = dbContext;
        this.salesService = salesService;
        this.purchaseService = purchaseService;
        this.stockService = stockService;
        this.userServices = userServices;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var startOfToday = today;
            var endOfToday = today.AddDays(1).AddTicks(-1);

            // Total Sales
            var totalSales = await dbContext.Sales.CountAsync();

            // Total Revenue
            var totalRevenue = await dbContext.Sales.SumAsync(s => s.TotalAmount);

            // Today's Revenue
            var todaysRevenue = await dbContext.Sales
                .Where(s => s.SalesDate >= startOfToday && s.SalesDate <= endOfToday)
                .SumAsync(s => s.TotalAmount);

            // Top 5 Selling Products (All Time)
            var topSellingProducts = await dbContext.SalesDetails
                .Include(sd => sd.Product)
                .GroupBy(sd => new { sd.ProductId, sd.Product.ProductName })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(sd => sd.Quantity)
                })
                .OrderByDescending(p => p.TotalQuantity)
                .Take(5)
                .ToListAsync();
            
            // Recent Sales (Last 5)
            var recentSales = await dbContext.Sales
                .Include(s => s.SalesDetails)
                .OrderByDescending(s => s.SalesDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TodaysRevenue = todaysRevenue;
            ViewBag.TopSellingProducts = topSellingProducts.Select(p => new { p.ProductName, p.TotalQuantity }).ToList();
            
            return View(recentSales);
        }
        catch (Exception ex)
        {
            // Log the exception (optional)
            TempData["Error"] = "An error occurred while loading dashboard data.";
            return View(new List<Sales>());
        }
    }

    // API endpoints for dashboard charts
    [HttpGet]
    public async Task<IActionResult> GetDailyData()
    {
        try
        {
            var currentDate = DateTime.UtcNow;
            var dailyData = new List<object>();

            for (int i = 6; i >= 0; i--)
            {
                var date = currentDate.AddDays(-i);
                var daySales = await dbContext.Sales
                    .Where(s => s.SalesDate.Date == date.Date)
                    .ToListAsync();

                var dayPurchases = await dbContext.Purchases
                    .Where(p => p.PurchaseDate.Date == date.Date)
                    .ToListAsync();

                dailyData.Add(new
                {
                    Date = date.ToString("MMM dd"),
                    Sales = daySales.Count,
                    Revenue = daySales.Sum(s => s.TotalAmount),
                    Purchases = dayPurchases.Count,
                    Costs = dayPurchases.Sum(p => p.TotalAmount),
                    Profit = daySales.Sum(s => s.TotalAmount) - dayPurchases.Sum(p => p.TotalAmount)
                });
            }

            return Json(dailyData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetStockChartData()
    {
        try
        {
            var stockData = await dbContext.Stocks
                .Include(s => s.Product)
                .Include(s => s.Product.Category)
                .GroupBy(s => s.Product.Category.CategoryName)
                .Select(g => new
                {
                    Category = g.Key,
                    Items = g.Count(),
                    Value = g.Sum(s => s.AvailableQuantity * s.UnitPrice)
                })
                .OrderByDescending(x => x.Value)
                .Take(8)
                .ToListAsync();

            return Json(stockData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTopProductsData()
    {
        try
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var topProducts = await dbContext.SalesDetails
                .Include(sd => sd.Product)
                .Where(sd => sd.Sales.SalesDate.Month == currentMonth && sd.Sales.SalesDate.Year == currentYear)
                .GroupBy(sd => sd.ProductId)
                .Select(g => new
                {
                    ProductName = g.First().Product.ProductName,
                    Quantity = g.Sum(sd => sd.Quantity),
                    Revenue = g.Sum(sd => sd.TotalPrice)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToListAsync();

            return Json(topProducts);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
