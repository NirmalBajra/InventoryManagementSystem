using System;
using InventoryManagementSystem.Constants;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Dto;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers;

public class AuthController : Controller
{
    private readonly FirstRunDbContext dbContext;
    private readonly IUserServices userServices;
    public AuthController(FirstRunDbContext dbContext, IUserServices userServices)
    {
        this.dbContext = dbContext;
        this.userServices = userServices;
    }
    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Register()
    {
        return View();
    }
    [HttpGet]
    public IActionResult Forbidden()
    {
        return View(); // Views/Auth/Forbidden.cshtml
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserVm vm)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            vm.UserStatus ??= UserStatusConstrants.Active;
            await userServices.CreateUser(vm);
            TempData["SuccessMessage"] = "User registered successfully.";
            return RedirectToAction("Login", "Auth");
        }
        catch (Exception e)
        {
            var errorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(vm);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        try
        {
            await userServices.Login(dto);
            return RedirectToAction("Index", "Dashboards");
        }
        catch (Exception e)
        {
            ModelState.AddModelError(string.Empty, $"Login failed: {e.Message}");
            return View(dto);
        }
    }
    public async Task<IActionResult> Logout()
    {
        await userServices.Logout();
        return RedirectToAction("Index", "Home");
    }
}
