using System;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class SupplierServices : ISupplierServices
{
    private readonly FirstRunDbContext dbContext;
    public SupplierServices(FirstRunDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    //Get all suppliers
    public async Task<IEnumerable<SupplierVm>> GetAllSuppliers()
    {
        return await dbContext.Suppliers
            .Select(s => new SupplierVm
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                Contact = s.Contact,
                Email = s.Email,
                Address = s.Address,
                CreatedAt = s.CreatedAt
            }).ToListAsync();
    }

    //get supplier id by id
    public async Task<SupplierVm> GetSupplierById(int id)
    {
        var supplier = await dbContext.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            throw new UserNotFoundException("No Supplier Found.");
        }
        return new SupplierVm
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            Contact = supplier.Contact,
            Email = supplier.Email,
            Address = supplier.Address,
            CreatedAt = supplier.CreatedAt
        };
    }

    //Create new Supplier
    public async Task CreateSupplier(SupplierVm vm)
    {
        var supplier = new Supplier
        {
            SupplierName = vm.SupplierName,
            Contact = vm.Contact,
            Email = vm.Email,
            Address = vm.Address,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Add(supplier);
        await dbContext.SaveChangesAsync();
    }

    //update suppliers
    public async Task UpdateSupplier(SupplierVm vm)
    {
        var supplier = await dbContext.Suppliers.FindAsync(vm.SupplierId);
        if (supplier == null)
        {
            throw new UserNotFoundException("Supplier Not found");
        }

        supplier.SupplierName = vm.SupplierName;
        supplier.Contact = vm.Contact;
        supplier.Email = vm.Email;
        supplier.Address = vm.Address;

        dbContext.Suppliers.Update(supplier);
        await dbContext.SaveChangesAsync();
    }

    //Delete Supplier
    public async Task DeleteSupplier(int id)
    {
        var supplier = await dbContext.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            throw new UserNotFoundException("Supplier not Found.");
        }
        dbContext.Remove(supplier);
        await dbContext.SaveChangesAsync();
    }
}
