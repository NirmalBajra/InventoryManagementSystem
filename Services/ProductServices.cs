using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Product;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class ProductServices : IProductServices
{
    private readonly FirstRunDbContext dbContext;
    public ProductServices(FirstRunDbContext dbContext){
        this.dbContext = dbContext;
    }

    //Create Product
    public async Task AddProduct(ProductCreateVm vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var product = new Product
        {
            ProductName = vm.ProductName,
            Description = vm.Description,
            CategoryId = vm.CategoryId
        };
        dbContext.Add(product);
        await dbContext.SaveChangesAsync();
        tnx.Complete();
    }

    //Get all Products
    public async Task<List<ProductVm>> GetAllProduct()
    {
        return await dbContext.Products
            .Include(p => p.Category)
            .Select(p => new ProductVm{
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                CategoryName = p.Category.CategoryName
            }).ToListAsync();
    }

    //Get Product By name
    public async Task<List<Product>> GetProductByName(string name)
    {
        return await dbContext.Products
            .Where(p => p.ProductName.Contains(name))
            .Include(p => p.Category)
            .ToListAsync();
    }

    //Update Product
    public async Task UpdateProduct(ProductUpdateVm vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var product = await dbContext.Products.FindAsync(vm.ProductId);
        if(product != null)
        {
            product.ProductName = vm.ProductName;
            product.Description = vm.Description;
            product.CategoryId = vm.CategoryId;

            await dbContext.SaveChangesAsync();
        }
        tnx.Complete();
    }

    //Delete Product
    public async Task DeleteProduct(int id)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var product = await dbContext.Products.FindAsync(id);
        if(product != null)
        {
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
        }
        tnx.Complete();
    }

    //DeleteMultiple Products
    public async Task DeleteMultipleProduct(List<int> productIds)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var product = await dbContext.Products.Where(p => productIds.Contains(p.ProductId)).ToListAsync();
        if(product.Any())
        {
            dbContext.Products.RemoveRange(product);
            await dbContext.SaveChangesAsync();
        }
        tnx.Complete();
    }
}
