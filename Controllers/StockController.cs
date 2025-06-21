using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class StockController : Controller
    {
        // GET: StockController
        public IActionResult ViewStock()
        {
            return View();
        }

    }
}
