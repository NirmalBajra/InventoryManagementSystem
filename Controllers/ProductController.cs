using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductServices productServices;
        private readonly ILogger<ProductController> logger;
        private readonly ProductCategoryServices productCategoryServices;
        public ProductController(ProductServices productServices, ProductCategoryServices productCategoryServices, ILogger<ProductController> logger)
        {
            this.productServices = productServices;
            this.productCategoryServices = productCategoryServices;
            this.logger = logger;
        }

        // GET: ProductController
        [Authorize]
        public async Task<ActionResult> ViewProduct(string searchString)
        {
            List<ProductVm> products;
            if (!string.IsNullOrEmpty(searchString))
            {
                products = await productServices.GetProductByName(searchString);
            }
            else
            {
                products = await productServices.GetAllProduct();
            }
            return View(products);
        }

        // GET: Product/AddProduct
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddProduct()
        {
            var categories = await productCategoryServices.GetAllProductCategory();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Product/AddProduct
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddProduct(ProductCreateVm vm)
        {
            if (ModelState.IsValid)
            {
                await productServices.AddProduct(vm);
                return RedirectToAction("ViewProduct");
            }

            var categories = await productCategoryServices.GetAllProductCategory();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            return View(vm);
        }

        // GET: Edit Product Page
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await productServices.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await productCategoryServices.GetAllProductCategory();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);

            var vm = new ProductUpdateVm
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryId = product.CategoryId,
                ImagePath = product.ImagePath
            };
            return View(vm);
        }

        // POST: Edit Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditProduct(ProductUpdateVm vm)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload if a new image is provided
                if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile.FileName);
                    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    Directory.CreateDirectory(directoryPath);

                    var filePath = Path.Combine(directoryPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.ImageFile.CopyToAsync(stream);
                    }

                    // Update ImagePath to the new image
                    vm.ImagePath = "images/" + fileName;
                }

                await productServices.UpdateProduct(vm);
                return RedirectToAction(nameof(ViewProduct));
            }

            var categories = await productCategoryServices.GetAllProductCategory();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", vm.CategoryId);

            return View(vm);
        }

        // POST: Delete Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await productServices.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            try
            {
                await productServices.DeleteProduct(id);
                TempData["SuccessMessage"] = "Product Deleted Successfully.";
                return RedirectToAction(nameof(ViewProduct));
            }
            catch (DbUpdateException dbEx)
            {
                logger.LogError(dbEx, "Database error while deleting product ID {ProductId}", id);
                TempData["ErrorMessage"] = "A database error occured. Please try again.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error deleting product Id {ProductId}", id);
                TempData["ErrorMessage"] = "There was an error deleting the product. Please try again.";
            }
            return RedirectToAction(nameof(ViewProduct));
        }


        // POST: Delete Multiple Products
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteMultipleProducts(List<int> productIds)
        {
            if (productIds != null && productIds.Any())
            {
                await productServices.DeleteMultipleProduct(productIds);
            }
            return RedirectToAction(nameof(ViewProduct));
        }
    }
}
