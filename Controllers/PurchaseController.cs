using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Dto;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Product;
using InventoryManagementSystem.ViewModels.Purchase;
using InventoryManagementSystem.ViewModels.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
namespace InventoryManagementSystem.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly FirstRunDbContext dbContext;
        private readonly IPurchaseService purchaseService;
        private readonly ISupplierServices supplierServices;
        private readonly IProductServices productServices;
        private readonly IStockService stockService;
        private readonly IMapper mapper;
        public PurchaseController(FirstRunDbContext dbContext, IPurchaseService purchaseService, ISupplierServices supplierServices, IProductServices productServices, IStockService stockService, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.purchaseService = purchaseService;
            this.supplierServices = supplierServices;
            this.productServices = productServices;
            this.stockService = stockService;
            this.mapper = mapper;
        }

        // GET: /Purchase/AddPurchase
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult AddPurchase()
        {
            ViewBag.Suppliers = new SelectList(dbContext.Suppliers, "SupplierId", "SupplierName");
            ViewBag.Products = new SelectList(dbContext.Products, "ProductId", "ProductName");
            ViewBag.Categories = new SelectList(dbContext.ProductCategories, "CategoryId", "CategoryName");
            var model = new PurchaseDto
            {
                PurchaseDate = DateTime.UtcNow
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddPurchase(PurchaseDto dto)
        {
            if (ModelState.IsValid)
            {
                // Overwrite any incoming date with the current server time
                dto.PurchaseDate = DateTime.UtcNow;
                await purchaseService.AddPurchaseAsync(dto);
                TempData["Success"] = "Purchase added successfully!";
                return RedirectToAction("ViewPurchases");
            }
            ViewBag.Suppliers = new SelectList(dbContext.Suppliers, "SupplierId", "SupplierName");
            ViewBag.Products = new SelectList(dbContext.Products, "ProductId", "ProductName");
            ViewBag.Categories = new SelectList(dbContext.ProductCategories, "CategoryId", " CategoryName");

            return View(dto);
        }

        // GET: /Purchase/ViewPurchases
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewPurchases(string? searchTerm = null, string? supplierFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, string sortBy = "PurchaseDate", string sortOrder = "desc")
        {
            try
            {
                var purchases = await purchaseService.GetAllPurchasesAsync();
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    purchases = purchases.Where(p => 
                        p.SupplierName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Apply supplier filter
                if (!string.IsNullOrEmpty(supplierFilter))
                {
                    purchases = purchases.Where(p => p.SupplierName == supplierFilter).ToList();
                }

                // Apply date range filter
                if (startDate.HasValue)
                {
                    purchases = purchases.Where(p => p.PurchaseDate.Date >= startDate.Value.Date).ToList();
                }
                if (endDate.HasValue)
                {
                    purchases = purchases.Where(p => p.PurchaseDate.Date <= endDate.Value.Date).ToList();
                }

                // Apply sorting
                purchases = sortBy.ToLower() switch
                {
                    "purchasedate" => sortOrder.ToLower() == "asc" 
                        ? purchases.OrderBy(p => p.PurchaseDate).ToList() 
                        : purchases.OrderByDescending(p => p.PurchaseDate).ToList(),
                    "suppliername" => sortOrder.ToLower() == "asc" 
                        ? purchases.OrderBy(p => p.SupplierName).ToList() 
                        : purchases.OrderByDescending(p => p.SupplierName).ToList(),
                    "totalamount" => sortOrder.ToLower() == "asc" 
                        ? purchases.OrderBy(p => p.TotalAmount).ToList() 
                        : purchases.OrderByDescending(p => p.TotalAmount).ToList(),
                    "purchaseid" => sortOrder.ToLower() == "asc" 
                        ? purchases.OrderBy(p => p.PurchaseId).ToList() 
                        : purchases.OrderByDescending(p => p.PurchaseId).ToList(),
                    _ => purchases.OrderByDescending(p => p.PurchaseDate).ToList()
                };

                // Apply pagination
                var totalItems = purchases.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var pagedPurchases = purchases.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Get unique suppliers for filter dropdown
                var suppliers = await supplierServices.GetAllSuppliers();
                ViewBag.Suppliers = suppliers.Select(s => s.SupplierName).Distinct().OrderBy(s => s).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SupplierFilter = supplierFilter;
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                ViewBag.SortBy = sortBy;
                ViewBag.SortOrder = sortOrder;

                return View(pagedPurchases);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new List<PurchaseListVm>());
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var purchase = await purchaseService.GetPurchaseByIdAsync(id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var purchase = await purchaseService.GetPurchaseByIdAsync(id);
            if (purchase == null) return NotFound();

            var dto = mapper.Map<PurchaseDto>(purchase);

            ViewBag.Suppliers = new SelectList(await supplierServices.GetAllSuppliers(), "SupplierId", "SupplierName");
            ViewBag.Products = new SelectList(await productServices.GetAllProduct(), "ProductId", "ProductName");

            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, PurchaseDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await purchaseService.UpdatePurchaseAsync(id, dto);
            return RedirectToAction("ViewPurchases");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await purchaseService.DeletePurchaseAsync(id);
            return RedirectToAction("ViewPurchases"); // Or wherever you list your purchases
        }
    }
}