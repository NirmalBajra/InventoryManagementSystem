using System;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.ViewModels.Product;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IProductServices
{
    Task AddProduct(ProductCreateVm vm);
    Task<List<ProductVm>> GetAllProduct();
    Task<List<Product>> GetProductByName(string name);
    Task UpdateProduct(ProductUpdateVm vm);
    Task DeleteProduct(int id);
    Task DeleteMultipleProduct(List<int> productIds);
}
