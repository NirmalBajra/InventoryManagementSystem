using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

public class SeedingController : Controller
{
    private readonly FirstRunDbContext dbContext;
    private readonly IUserServices userServices;

    public SeedingController(FirstRunDbContext dbContext,IUserServices userServices)
    {
        this.dbContext = dbContext;
        this.userServices = userServices;
    }

    public async Task<IActionResult> SeedSuperAdmin()
    {
        try{
            var previousSuperAdminExists = await DoesPreviousSuperAdminExists();
            if(!previousSuperAdminExists){
                using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                var adminVm = new UserVm()
                {
                    Email = "super.admin@gmail.com",
                    Role = UserTypeConstrants.Admin,
                    UserName = "Super Admin",
                    Password = "admin",
                    Address = "System",
                    Phone = "9800000000",
                    DOB = DateTime.UtcNow.AddYears(-27)                
                };
                await userServices.CreateUser(adminVm);
                txn.Complete();
            };
            return Content("User Seeding Complete");
        }
        catch(Exception e){
            return Content($"Seeding Failed: {e.Message}");
        }
    }

    private async Task<bool> DoesPreviousSuperAdminExists()
    {
        return await dbContext.Users.AnyAsync(u => u.Email == "super.admin@gmail.com");
    }
}
