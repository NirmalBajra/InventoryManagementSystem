using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class ChartController : Controller
    {
        private readonly IStockService stockService;
        public ChartController(IStockService stockService)
        {
            this.stockService = stockService;
        }
        // GET: ChartController
        public IActionResult SalesReport()
        {
            return View();
        }

        public IActionResult PurchaseReport()
        {
            return View();
        }
        public IActionResult StockReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> StockFlow()
        {
            try
            {
                var result = await stockService.ViewStockAsync();
                return View(result);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured.", details = ex.Message });
            }
        }
    }
}
