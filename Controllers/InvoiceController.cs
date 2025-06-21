using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Entity;

namespace InventoryManagementSystem.Controllers;

[Authorize]
public class InvoiceController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly ISalesService _salesService;

    public InvoiceController(IInvoiceService invoiceService, ISalesService salesService)
    {
        _invoiceService = invoiceService;
        _salesService = salesService;
    }

    public async Task<IActionResult> Index()
    {
        var invoices = await _invoiceService.GetAllInvoicesAsync();
        return View(invoices);
    }

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }
        return View(invoice);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GenerateInvoice(int salesId, string customerAddress = "", string customerPhone = "", decimal taxRate = 0, decimal discountAmount = 0, string notes = "")
    {
        try
        {
            var invoice = await _invoiceService.GenerateInvoiceAsync(salesId, customerAddress, customerPhone, taxRate, discountAmount, notes);
            TempData["SuccessMessage"] = $"Invoice {invoice.InvoiceNumber} generated successfully.";
            return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating invoice: {ex.Message}";
            return RedirectToAction("Details", "Sales", new { id = salesId });
        }
    }

    public async Task<IActionResult> DownloadPdf(int id)
    {
        try
        {
            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(id);
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            
            return File(pdfBytes, "application/pdf", $"Invoice_{invoice.InvoiceNumber}.pdf");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating PDF: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    public async Task<IActionResult> Print(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }
        return View(invoice);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdatePaymentStatus(int invoiceId, string paymentStatus, string paymentMethod = "")
    {
        try
        {
            await _invoiceService.UpdatePaymentStatusAsync(invoiceId, paymentStatus, paymentMethod);
            TempData["SuccessMessage"] = "Payment status updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating payment status: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Details), new { id = invoiceId });
    }

    public async Task<IActionResult> MonthlyReport(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (!startDate.HasValue)
            startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        else if (startDate.Value.Kind != DateTimeKind.Utc)
            startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
        
        if (!endDate.HasValue)
            endDate = startDate.Value.AddMonths(1).AddDays(-1);
        else if (endDate.Value.Kind != DateTimeKind.Utc)
            endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);

        var invoices = await _invoiceService.GetInvoicesByDateRangeAsync(startDate.Value, endDate.Value);
        
        ViewBag.StartDate = startDate.Value;
        ViewBag.EndDate = endDate.Value;
        ViewBag.TotalAmount = invoices.Sum(i => i.TotalAmount);
        ViewBag.PaidAmount = invoices.Where(i => i.PaymentStatus == "Paid").Sum(i => i.TotalAmount);
        ViewBag.PendingAmount = invoices.Where(i => i.PaymentStatus == "Pending").Sum(i => i.TotalAmount);
        
        return View(invoices);
    }
} 