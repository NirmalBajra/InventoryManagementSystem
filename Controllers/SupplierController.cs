using System;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers;
public class SupplierController : Controller
{
    private readonly FirstRunDbContext dbContext;
    private readonly SupplierServices supplierServices;
    public SupplierController(FirstRunDbContext dbContext, SupplierServices supplierServices)
    {
        this.dbContext = dbContext;
        this.supplierServices = supplierServices;
    }

    //Get: Supplier/AddSupplier
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> AddSupplier()
    {
        return View();
    }

    //POST: Supplier/AddSupplier
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> AddSupplier(SupplierVm vm)
    {
        if(ModelState.IsValid)
        {
            await supplierServices.CreateSupplier(vm);
            return RedirectToAction("ViewSupplier");
        }
        return View(vm);
    }

    //Get: ProductCategory
    [Authorize]
    public async Task<IActionResult> ViewSupplier()
    {
        var supplier = await supplierServices.GetAllSuppliers();
        if(supplier == null)
        {
            return NotFound();
        }
        return View(supplier);
    }

    //GET: Edit Suppliers
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditSupplier(int id)
    {
        var supplier = await supplierServices.GetSupplierById(id);
        if(supplier == null)
        {
            return NotFound();
        }

        var vm = new SupplierVm
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            Contact = supplier.Contact,
            Email = supplier.Email,
            Address = supplier.Address
        };
        return View(vm);
    }

    //Post: Edit Supplier
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditSupplier(SupplierVm vm)
    {
        if(ModelState.IsValid)
        {
            await supplierServices.UpdateSupplier(vm);
            return RedirectToAction(nameof(ViewSupplier));
        }
        return View(vm);
    }

    //Delete Suppliers
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        await supplierServices.DeleteSupplier(id);
        return RedirectToAction(nameof(ViewSupplier));
    }
}
