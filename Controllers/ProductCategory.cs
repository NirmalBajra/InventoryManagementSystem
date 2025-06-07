using System.Threading.Tasks;
using InventoryManagementSystem.Entity;
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
            if (category == null)
            {
                return NotFound();
            }
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

        //Get: Edit Product Category Page
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditProductCategory(int id)
        {
            var category = await productCategoryServices.GetProductCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }

            var vm = new ProductCategoryUpdateVM
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
            };
            return View(vm);
        }

        //Post: Edit Product Category
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditProductCategory(ProductCategoryUpdateVM vm)
        {
            if (ModelState.IsValid)
            {
                await productCategoryServices.UpdateProductCategory(vm);
                return RedirectToAction(nameof(ViewProductCategory));
            }
            return View(vm);
        }

        //Delete Product Category - POST
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteProductCategory(int id)
        {
            await productCategoryServices.DeleteProductCategory(id);
            return RedirectToAction(nameof(ViewProductCategory));
        }
    }
}
