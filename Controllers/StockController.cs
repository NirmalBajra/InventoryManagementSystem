using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class StockController : Controller
    {
        // GET: StockController
        public IActionResult ViewStock()
        {
            return View();
        }

    }
}
