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
}
