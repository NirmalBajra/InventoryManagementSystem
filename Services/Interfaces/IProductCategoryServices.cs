using System;
using InventoryManagementSystem.ViewModels.ProductCategory;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IProductCategoryServices
{
    Task AddProductCategory(ProductCategoryCreateVM vm);
    Task UpdateProductCategory(ProductCategoryUpdateVM vm);
    Task DeleteProductCategory(int id);
    Task<List<ProductCategoryVM>> GetAllProductCategory();
    Task<ProductCategoryVM> GetProductCategoryByName(string name);
    Task<ProductCategoryVM> GetProductCategoryById(int id);
    Task DeleteMultipleProductCategories(List<int> categoryIds);
}
