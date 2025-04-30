using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Product;
using InventoryManagementSystem.ViewModels.Purchase;
using InventoryManagementSystem.ViewModels.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly FirstRunDbContext dbContext;
        private readonly IPurchaseService purchaseService;
        private readonly ISupplierServices supplierServices;
        private readonly IProductServices productServices;
        private readonly IStockService stockService;
        public PurchaseController(FirstRunDbContext dbContext, IPurchaseService purchaseService, ISupplierServices supplierServices, IProductServices productServices, IStockService stockService)
        {
            this.dbContext = dbContext;
            this.purchaseService = purchaseService;
            this.supplierServices = supplierServices;
            this.productServices = productServices;
            this.stockService = stockService;
        }

        // GET: /Purchase/AddPurchase
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult AddPurchase()
        {
            var products = dbContext.Products
                .Select(p => new ProductVm
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName
                }).ToList();

            var suppliers = dbContext.Suppliers
                .Select(s => new SupplierVm
                {
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierName,
                }).ToList();

            var vm = new PurchaseVm
            {
                Products = products,
                Suppliers = suppliers
            };

            return View(vm);
        }

        // POST: /Purchase/AddPurchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddPurchase(PurchaseVm vm)
        {
            if (!ModelState.IsValid)
            {
                var products = dbContext.Products.Select(p => new ProductVm
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName
                }).ToList();

                var suppliers = dbContext.Suppliers.Select(s => new SupplierVm
                {
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierName
                }).ToList();

                vm.Products = products;
                vm.Suppliers = suppliers;

                return View(vm);
            }

            await purchaseService.AddPurchase(vm);
            return RedirectToAction(nameof(ViewPurchases));
        }

        // GET: /Purchase/ViewPurchases
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewPurchases()
        {
            var purchases = await purchaseService.GetAllPurchases(); 
            return View(purchases);
        }
    }
}