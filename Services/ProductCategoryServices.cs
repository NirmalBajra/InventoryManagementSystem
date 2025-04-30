using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.ProductCategory;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class ProductCategoryServices : IProductCategoryServices
{
    private readonly FirstRunDbContext dbContext;
    public ProductCategoryServices(FirstRunDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    //Add Product Category
    public async Task AddProductCategory(ProductCategoryCreateVM vm)
    {
        using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var category = new ProductCategory
        {
            CategoryName = vm.CategoryName,
            Description = vm.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        txn.Complete();
    }

    //Update ProductCategory
    public async Task UpdateProductCategory(ProductCategoryUpdateVM vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var category = await dbContext.ProductCategories.FindAsync(vm.CategoryId);
        if (category != null)
        {
            category.CategoryName = vm.CategoryName;
            category.Description = vm.Description;
            category.IsActive = vm.IsActive;

            await dbContext.SaveChangesAsync();
        }
        tnx.Complete();
    }

    //Delete Product Category
    public async Task DeleteProductCategory(int id)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var category = await dbContext.ProductCategories.FindAsync(id);
        if (category != null)
        {
            dbContext.ProductCategories.Remove(category);
            await dbContext.SaveChangesAsync();
        }
        tnx.Complete();
    }

    //Delete Multiple ProductCategories
    public async Task DeleteMultipleProductCategories(List<int> categoryIds)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var categories = await dbContext.ProductCategories.Where(c => categoryIds.Contains(c.CategoryId)).ToListAsync();

        if (categories.Any())
        {
            dbContext.ProductCategories.RemoveRange(categories);
            await dbContext.SaveChangesAsync();
        }
        tnx.Complete();
    }

    // View Product Category
    public async Task<List<ProductCategoryVM>> GetAllProductCategory()
    {
        return await dbContext.ProductCategories
            .Select(c => new ProductCategoryVM
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToListAsync();
    }

    //View ProductCategoryByName
    public async Task<ProductCategoryVM> GetProductCategoryByName(string name)
    {
        var result = await dbContext.ProductCategories
            .Where(c => c.CategoryName == name)
            .Select(c => new ProductCategoryVM
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).FirstOrDefaultAsync();
        
        if(result == null )
        {
            throw new UserNotFoundException($"Product Category not found.");
        }
        return result;
    }

    //Get Product By id
    public async Task<ProductCategoryVM> GetProductCategoryById(int id)
    {
        var result = await dbContext.ProductCategories
            .Where(c => c.CategoryId == id)
            .Select(c => new ProductCategoryVM
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).FirstOrDefaultAsync();

        if(result == null)
        {
            throw new UserNotFoundException($"Product Category Not Found");
        }
        return result;
    }
}
