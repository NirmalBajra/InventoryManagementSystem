using System.Threading.Tasks;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels.ProductCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class ProductCategory : Controller
    {
        private readonly ProductCategoryServices productCategoryServices;
        public ProductCategory(ProductCategoryServices productCategoryServices)
        {
            this.productCategoryServices = productCategoryServices;
        }

        //Get: ProductCategory
        [Authorize]
        public async Task<ActionResult> ViewProductCategory()
        {
            var category = await productCategoryServices.GetAllProductCategory();
            return View(category);
        }


        // GET: ProductCategory/AddProductCategory
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult AddProductCategory()
        {
            return View();
        }

        //Post: ProductCategory/AddProductCategory
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> AddProductCategory(ProductCategoryCreateVM vm)
        {
            if (ModelState.IsValid)
            {
                await productCategoryServices.AddProductCategory(vm);
                return RedirectToAction("ViewProductCategory"); 
            }
            return View(vm);
        }

    }
}
