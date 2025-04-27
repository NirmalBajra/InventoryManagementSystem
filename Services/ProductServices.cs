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
    public ProductServices(FirstRunDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    //Create Product
    public async Task AddProduct(ProductCreateVm vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        string? imagePath = null;

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
            imagePath = "images/" + fileName;
        }
        var product = new Product
        {
            ProductName = vm.ProductName,
            Description = vm.Description,
            CategoryId = vm.CategoryId,
            ImagePath = imagePath
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
            .Select(p => new ProductVm
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                CategoryName = p.Category.CategoryName,
                ImagePath = string.IsNullOrEmpty(p.ImagePath) ? "images/default.png" : p.ImagePath,
                CategoryId = p.CategoryId
            }).ToListAsync();
    }

    //Get Product By name
    public async Task<List<ProductVm>> GetProductByName(string name)
    {
        return await dbContext.Products
        .Include(p => p.Category)
        .Select(p => new ProductVm
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Description = p.Description,
            CategoryName = p.Category.CategoryName,   // Mapping from Product to ProductVm
            ImagePath = p.ImagePath ?? "images/default.png",  // Default image if no image is provided
            CategoryId = p.CategoryId
        })
        .ToListAsync();
    }

    //Get Product By Id
    public async Task<ProductVm> GetProductById(int id)
    {
        return await dbContext.Products
        .Where(p => p.ProductId == id)
        .Include(p => p.Category)
        .Select(p => new ProductVm
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Description = p.Description,
            CategoryName = p.Category.CategoryName,
            ImagePath = string.IsNullOrEmpty(p.ImagePath) ? "images/default.png" : p.ImagePath,
            CategoryId = p.CategoryId
        })
        .FirstOrDefaultAsync();
    }


    //Update Product
    public async Task UpdateProduct(ProductUpdateVm vm)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var product = await dbContext.Products.FindAsync(vm.ProductId);
        if (product != null)
        {
            product.ProductName = vm.ProductName;
            product.Description = vm.Description;
            product.CategoryId = vm.CategoryId;

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
                product.ImagePath = "images/" + fileName;
            }
            await dbContext.SaveChangesAsync();
        }
        tnx.Complete();
    }

    //Delete Product
    public async Task DeleteProduct(int id)
    {
        var product = await dbContext.Products.FindAsync(id);
        if (product != null)
        {
            //remove the image from local file
            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                var fullImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePath);
                if (File.Exists(fullImagePath))
                {
                    File.Delete(fullImagePath);
                }
            }
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
        }
    }

    //DeleteMultiple Products
    public async Task DeleteMultipleProduct(List<int> productIds)
    {
        using var tnx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var product = await dbContext.Products.Where(p => productIds.Contains(p.ProductId)).ToListAsync();
        if (product.Any())
        {
            //delete the images from the local file
            foreach (var prod in product)
            {
                if (!string.IsNullOrEmpty(prod.ImagePath))
                {
                    var fullImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", prod.ImagePath);
                    if (File.Exists(fullImagePath))
                    {
                        File.Delete(fullImagePath);
                    }
                }
            }
            dbContext.Products.RemoveRange(product);
            await dbContext.SaveChangesAsync();
        }
        tnx.Complete();
    }
}
