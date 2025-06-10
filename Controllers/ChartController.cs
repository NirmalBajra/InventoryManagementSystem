using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class ChartController : Controller
    {
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
        public IActionResult StockFlow()
        {
            return View();
        }
    }
}
