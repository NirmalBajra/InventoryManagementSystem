using InventoryManagementSystem.Data;
using InventoryManagementSystem.Dtos.SalesDtos;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly FirstRunDbContext dbContext;
        private readonly ISalesService salesService;
        private readonly IProductServices productServices;

        public SalesController(FirstRunDbContext dbContext, ISalesService salesService, IProductServices productServices)
        {
            this.dbContext = dbContext;
            this.salesService = salesService;
            this.productServices = productServices;
        }

        [HttpGet]
        public async Task<IActionResult> Index(SalesFilterDto filter)
        {
            var (sales, totalRecords) = await salesService.GetAllSalesAsync(filter);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.Filter = filter;
            return View(sales);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateSales()
        {
            try
            {
                var products = await productServices.GetAllProductsAsync();
                var viewModel = new SalesVm
                {
                    Products = products,
                    SalesDate = DateTime.UtcNow,
                    SalesDetails = new List<SalesDetailVm>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateSales(SalesVm model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var products = await productServices.GetAllProductsAsync();
                    model.Products = products;
                    return View(model);
                }

                // Ensure SalesDate is UTC
                if (model.SalesDate.Kind == DateTimeKind.Unspecified)
                {
                    model.SalesDate = DateTime.SpecifyKind(model.SalesDate, DateTimeKind.Utc);
                }
                else if (model.SalesDate.Kind == DateTimeKind.Local)
                {
                    model.SalesDate = model.SalesDate.ToUniversalTime();
                }

                var createSalesDto = new CreateSalesDto
                {
                    CustomerName = model.CustomerName,
                    SalesDate = model.SalesDate,
                    SalesDetails = model.SalesDetails.Select(sd => new SalesDetailDto
                    {
                        ProductId = sd.ProductId,
                        Quantity = sd.Quantity,
                        UnitPrice = sd.UnitPrice
                    }).ToList()
                };

                await salesService.CreateSalesAsync(createSalesDto, User.Identity?.Name ?? "System");
                TempData["Success"] = "Sale created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var products = await productServices.GetAllProductsAsync();
                model.Products = products;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var sale = await salesService.GetSalesByIdAsync(id);
                return View(sale);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditSales(int id)
        {
            var sale = await salesService.GetSalesByIdAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            var products = await productServices.GetAllProductsAsync();
            var model = new SalesVm
            {
                SalesId = sale.SalesId,
                CustomerName = sale.CustomerName,
                SalesDate = sale.SalesDate,
                Products = products,
                SalesDetails = sale.SalesDetails.Select(sd => new SalesDetailVm
                {
                    ProductId = sd.ProductId,
                    Quantity = sd.Quantity,
                    UnitPrice = sd.UnitPrice,
                    ProductName = sd.Product?.ProductName ?? ""
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditSales(int id, SalesVm model)
        {
            if (!ModelState.IsValid)
            {
                var products = await productServices.GetAllProductsAsync();
                model.Products = products;
                return View(model);
            }

            var createSalesDto = new CreateSalesDto
            {
                CustomerName = model.CustomerName,
                SalesDate = model.SalesDate,
                SalesDetails = model.SalesDetails.Select(sd => new SalesDetailDto
                {
                    ProductId = sd.ProductId,
                    Quantity = sd.Quantity,
                    UnitPrice = sd.UnitPrice
                }).ToList()
            };

            await salesService.UpdateSalesAsync(id, createSalesDto, User.Identity?.Name ?? "System");
            TempData["Success"] = "Sale updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await salesService.DeleteSalesAsync(id, User.Identity?.Name ?? "System");
                TempData["Success"] = "Sale deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            try
            {
                var sale = await salesService.GetSalesByIdAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Sale not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if invoice already exists
                var existingInvoice = await dbContext.Invoices
                    .FirstOrDefaultAsync(i => i.SalesId == id);

                if (existingInvoice != null)
                {
                    // Redirect to invoice details if it exists
                    return RedirectToAction("Details", "Invoice", new { id = existingInvoice.InvoiceId });
                }

                // Generate new invoice
                var invoiceService = HttpContext.RequestServices.GetRequiredService<IInvoiceService>();
                var invoice = await invoiceService.GenerateInvoiceAsync(id);
                
                TempData["Success"] = "Invoice generated successfully!";
                return RedirectToAction("Details", "Invoice", new { id = invoice.InvoiceId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            try
            {
                var sale = await salesService.GetSalesByIdAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Sale not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(sale);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
