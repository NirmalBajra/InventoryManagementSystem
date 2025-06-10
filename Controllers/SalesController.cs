using InventoryManagementSystem.Data;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class SalesController : Controller
    {
        private readonly FirstRunDbContext dbContext;
        private readonly ISalesService salesService;
        private readonly IStockService stockService;
        public SalesController(FirstRunDbContext dbContext, ISalesService salesService, IStockService stockService)
        {
            this.dbContext = dbContext;
            this.salesService = salesService;
            this.stockService = stockService;
        }
        [HttpGet]
        public IActionResult CreateSales()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult ViewSales()
        {
            return View();
        }
        // GET: SalesController
        public ActionResult Index()
        {
            return View();
        }

    }
}
