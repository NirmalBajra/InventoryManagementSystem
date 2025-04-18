using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers;

public class DashboardsController : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

}
