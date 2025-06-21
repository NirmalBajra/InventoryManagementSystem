using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Services.Interfaces;

namespace InventoryManagementSystem.Controllers;

[Authorize]
public class StockAlertController : Controller
{
    private readonly IStockAlertService _stockAlertService;

    public StockAlertController(IStockAlertService stockAlertService)
    {
        _stockAlertService = stockAlertService;
    }

    public async Task<IActionResult> Index()
    {
        var alerts = await _stockAlertService.GetAllAlertsAsync();
        return View(alerts);
    }

    public async Task<IActionResult> UnreadAlerts()
    {
        var alerts = await _stockAlertService.GetUnreadAlertsAsync();
        return View(alerts);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int alertId)
    {
        await _stockAlertService.MarkAlertAsReadAsync(alertId);
        return RedirectToAction(nameof(UnreadAlerts));
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _stockAlertService.MarkAllAlertsAsReadAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _stockAlertService.GetUnreadAlertCountAsync();
        return Json(new { count });
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadAlertsJson()
    {
        var alerts = await _stockAlertService.GetUnreadAlertsAsync();
        var alertData = alerts.Select(a => new
        {
            id = a.AlertId,
            productName = a.Product?.ProductName ?? "Unknown Product",
            alertType = a.AlertType,
            message = a.Message,
            createdAt = a.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
            iconClass = a.AlertType == "LowStock" ? "fa-exclamation-triangle" : "fa-times-circle",
            iconColor = a.AlertType == "LowStock" ? "notif-warning" : "notif-danger"
        }).ToList();
        
        return Json(alertData);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CheckStockAlerts()
    {
        await _stockAlertService.CheckAndCreateStockAlertsAsync();
        TempData["SuccessMessage"] = "Stock alerts checked and updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateTestAlerts()
    {
        // Create some test alerts for demonstration
        await _stockAlertService.CreateLowStockAlertAsync(1, 5, 10);
        await _stockAlertService.CreateOutOfStockAlertAsync(2);
        await _stockAlertService.CreateLowStockAlertAsync(3, 3, 10);
        
        TempData["SuccessMessage"] = "Test alerts created successfully.";
        return RedirectToAction(nameof(Index));
    }
} 