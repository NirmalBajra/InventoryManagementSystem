using InventoryManagementSystem.Entity;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> GenerateInvoiceAsync(int salesId, string customerAddress = "", string customerPhone = "", decimal taxRate = 0, decimal discountAmount = 0, string notes = "");
    Task<Invoice> GetInvoiceByIdAsync(int invoiceId);
    Task<Invoice> GetInvoiceBySalesIdAsync(int salesId);
    Task<List<Invoice>> GetAllInvoicesAsync();
    Task<List<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task UpdatePaymentStatusAsync(int invoiceId, string paymentStatus, string paymentMethod = "");
    Task<string> GenerateInvoiceNumberAsync();
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);
} 