using InventoryManagementSystem.Data;
using InventoryManagementSystem.Dtos.SalesDtos;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class ChartController : Controller
    {
        private readonly IStockService stockService;
        private readonly ISalesService salesService;
        private readonly IPurchaseService purchaseService;
        private readonly FirstRunDbContext dbContext;

        public ChartController(IStockService stockService, ISalesService salesService, IPurchaseService purchaseService, FirstRunDbContext dbContext)
        {
            this.stockService = stockService;
            this.salesService = salesService;
            this.purchaseService = purchaseService;
            this.dbContext = dbContext;
        }

        // GET: ChartController
        public async Task<IActionResult> SalesReport(DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Set default date range to last 30 days if not provided
                if (!startDate.HasValue)
                    startDate = DateTime.UtcNow.AddDays(-30).Date;
                if (!endDate.HasValue)
                    endDate = DateTime.UtcNow.Date;

                var query = dbContext.Sales
                    .Include(s => s.SalesDetails)
                    .ThenInclude(sd => sd.Product)
                    .ThenInclude(p => p.Category)
                    .Where(s => s.SalesDate >= startDate.Value && s.SalesDate <= endDate.Value.AddDays(1).AddSeconds(-1));

                // Get total count for pagination
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Apply pagination
                var salesData = await query
                    .OrderByDescending(s => s.SalesDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.SalesData = salesData;
                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading sales report: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> PurchaseReport(DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Set default date range to last 30 days if not provided
                if (!startDate.HasValue)
                    startDate = DateTime.UtcNow.AddDays(-30).Date;
                if (!endDate.HasValue)
                    endDate = DateTime.UtcNow.Date;

                var query = dbContext.Purchases
                    .Include(p => p.Supplier)
                    .Include(p => p.PurchaseDetails)
                    .ThenInclude(pd => pd.Product)
                    .ThenInclude(p => p.Category)
                    .Where(p => p.PurchaseDate >= startDate.Value && p.PurchaseDate <= endDate.Value.AddDays(1).AddSeconds(-1));

                // Get total count for pagination
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Apply pagination
                var purchaseData = await query
                    .OrderByDescending(p => p.PurchaseDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.PurchaseData = purchaseData;
                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading purchase report: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> StockReport()
        {
            try
            {
                var stockData = await dbContext.Stocks
                    .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                    .Select(s => new 
                    {
                        ProductName = s.Product.ProductName,
                        CategoryName = s.Product.Category.CategoryName,
                        Quantity = s.Quantity,
                        UnitPrice = s.UnitPrice
                    })
                    .ToListAsync();

                return View(stockData);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading stock report: {ex.Message}";
                return View(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> StockFlow(string filterType = "", string searchTerm = "", int page = 1, int pageSize = 10, string sortBy = "Date", string sortOrder = "desc", DateTime? selectedDate = null)
        {
            try
            {
                var query = dbContext.StockFlows.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(sf => sf.Product.ProductName.Contains(searchTerm) || sf.Product.Category.CategoryName.Contains(searchTerm));
                }
                
                if (!string.IsNullOrEmpty(filterType))
                {
                    if (filterType == "in")
                    {
                        query = query.Where(sf => sf.StockType == Enums.StockType.In);
                    }
                    else if (filterType == "out")
                    {
                        query = query.Where(sf => sf.StockType == Enums.StockType.Out);
                    }
                }

                // Add date filtering with proper UTC conversion
                if (selectedDate.HasValue)
                {
                    // Convert the selected date to UTC
                    var startOfDay = DateTime.SpecifyKind(selectedDate.Value.Date, DateTimeKind.Utc);
                    var endOfDay = startOfDay.AddDays(1);
                    query = query.Where(sf => sf.CreatedAt >= startOfDay && sf.CreatedAt < endOfDay);
                }

                switch (sortBy.ToLower())
                {
                    case "product":
                        query = sortOrder == "asc" ? query.OrderBy(s => s.Product.ProductName) : query.OrderByDescending(s => s.Product.ProductName);
                        break;
                    case "type":
                        query = sortOrder == "asc" ? query.OrderBy(s => s.StockType) : query.OrderByDescending(s => s.StockType);
                        break;
                    case "quantity":
                        query = sortOrder == "asc" ? query.OrderBy(s => s.Quantity) : query.OrderByDescending(s => s.Quantity);
                        break;
                    default:
                        query = sortOrder == "asc" ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt);
                        break;
                }

                var totalItems = await query.CountAsync();
                var stockFlowData = await query
                    .Include(sf => sf.Product)
                    .ThenInclude(p => p.Category)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                ViewBag.StockFlowData = stockFlowData;
                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                ViewBag.FilterType = filterType;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SortBy = sortBy;
                ViewBag.SortOrder = sortOrder;
                ViewBag.SelectedDate = selectedDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading stock flow: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProfitLossReport()
        {
            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                // Calculate sales revenue
                var salesRevenue = await dbContext.Sales
                    .Where(s => s.SalesDate.Month == currentMonth && s.SalesDate.Year == currentYear)
                    .SumAsync(s => s.TotalAmount);

                // Calculate purchase costs
                var purchaseCosts = await dbContext.Purchases
                    .Where(p => p.PurchaseDate.Month == currentMonth && p.PurchaseDate.Year == currentYear)
                    .SumAsync(p => p.TotalAmount);

                // Calculate gross profit
                var grossProfit = salesRevenue - purchaseCosts;
                var profitMargin = salesRevenue > 0 ? (grossProfit / salesRevenue) * 100 : 0;

                // Get monthly comparison
                var monthlyData = new List<object>();
                for (int i = 5; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddMonths(-i);
                    var monthSales = await dbContext.Sales
                        .Where(s => s.SalesDate.Month == date.Month && s.SalesDate.Year == date.Year)
                        .SumAsync(s => s.TotalAmount);
                    var monthPurchases = await dbContext.Purchases
                        .Where(p => p.PurchaseDate.Month == date.Month && p.PurchaseDate.Year == date.Year)
                        .SumAsync(p => p.TotalAmount);

                    monthlyData.Add(new
                    {
                        Month = date.ToString("MMM yyyy"),
                        Sales = monthSales,
                        Purchases = monthPurchases,
                        Profit = monthSales - monthPurchases
                    });
                }

                ViewBag.SalesRevenue = salesRevenue;
                ViewBag.PurchaseCosts = purchaseCosts;
                ViewBag.GrossProfit = grossProfit;
                ViewBag.ProfitMargin = profitMargin;
                ViewBag.MonthlyData = monthlyData;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
        }

        // API endpoints for chart data
        [HttpGet]
        public async Task<IActionResult> GetSalesChartData()
        {
            try
            {
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var salesData = await dbContext.Sales
                    .Where(s => s.SalesDate >= sixMonthsAgo)
                    .GroupBy(s => new { s.SalesDate.Month, s.SalesDate.Year })
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Sales = g.Count(),
                        Revenue = g.Sum(s => s.TotalAmount)
                    })
                    .OrderBy(x => x.Month)
                    .ToListAsync();

                return Json(salesData);
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
                var topProducts = await dbContext.SalesDetails
                    .Include(sd => sd.Product)
                    .GroupBy(sd => sd.ProductId)
                    .Select(g => new
                    {
                        ProductName = g.First().Product.ProductName,
                        Quantity = g.Sum(sd => sd.Quantity),
                        Revenue = g.Sum(sd => sd.TotalPrice)
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(10)
                    .ToListAsync();

                return Json(topProducts);
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
                    .ToListAsync();

                return Json(stockData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
