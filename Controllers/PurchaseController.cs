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
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddPurchase(PurchaseDto dto)
        {
            if (ModelState.IsValid)
            {
                await purchaseService.AddPurchaseAsync(dto);
                return RedirectToAction("AddPurchase");
            }
            ViewBag.Suppliers = new SelectList(dbContext.Suppliers, "SupplierId", "SupplierName");
            ViewBag.Products = new SelectList(dbContext.Products, "ProductId", "ProductName");
            ViewBag.Categories = new SelectList(dbContext.ProductCategories, "CategoryId", " CategoryName");

            return View(dto);
        }

        // GET: /Purchase/ViewPurchases
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewPurchases()
        {
            var purchases = await purchaseService.GetAllPurchasesAsync();
            return View(purchases);
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
        public async Task<IActionResult> Edit(int id, PurchaseDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await purchaseService.UpdatePurchaseAsync(id, dto);
            return RedirectToAction("ViewPurchases");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await purchaseService.DeletePurchaseAsync(id);
            return RedirectToAction("ViewPurchases"); // Or wherever you list your purchases
        }
    }
}