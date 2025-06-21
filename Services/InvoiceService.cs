using InventoryManagementSystem.Data;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class InvoiceService : IInvoiceService
{
    private readonly FirstRunDbContext _dbContext;

    public InvoiceService(FirstRunDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Invoice> GenerateInvoiceAsync(int salesId, string customerAddress = "", string customerPhone = "", decimal taxRate = 0, decimal discountAmount = 0, string notes = "")
    {
        var sales = await _dbContext.Sales
            .Include(s => s.SalesDetails)
            .ThenInclude(sd => sd.Product)
            .FirstOrDefaultAsync(s => s.SalesId == salesId);

        if (sales == null)
            throw new ArgumentException("Sales record not found");

        var existingInvoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.SalesId == salesId);

        if (existingInvoice != null)
            return existingInvoice;

        var invoiceNumber = await GenerateInvoiceNumberAsync();
        var subTotal = sales.TotalAmount;
        var taxAmount = subTotal * (taxRate / 100);
        var totalAmount = subTotal + taxAmount - discountAmount;

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            SalesId = salesId,
            CustomerName = sales.CustomerName,
            CustomerAddress = customerAddress,
            CustomerPhone = customerPhone,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30), // 30 days payment terms
            PaymentStatus = "Pending",
            Notes = notes
        };

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        return invoice;
    }

    public async Task<Invoice> GetInvoiceByIdAsync(int invoiceId)
    {
        return await _dbContext.Invoices
            .Include(i => i.Sales)
            .ThenInclude(s => s.SalesDetails)
            .ThenInclude(sd => sd.Product)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<Invoice> GetInvoiceBySalesIdAsync(int salesId)
    {
        return await _dbContext.Invoices
            .Include(i => i.Sales)
            .ThenInclude(s => s.SalesDetails)
            .ThenInclude(sd => sd.Product)
            .FirstOrDefaultAsync(i => i.SalesId == salesId);
    }

    public async Task<List<Invoice>> GetAllInvoicesAsync()
    {
        return await _dbContext.Invoices
            .Include(i => i.Sales)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbContext.Invoices
            .Include(i => i.Sales)
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task UpdatePaymentStatusAsync(int invoiceId, string paymentStatus, string paymentMethod = "")
    {
        var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
        if (invoice != null)
        {
            invoice.PaymentStatus = paymentStatus;
            if (!string.IsNullOrEmpty(paymentMethod))
                invoice.PaymentMethod = paymentMethod;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var lastInvoice = await _dbContext.Invoices
            .OrderByDescending(i => i.InvoiceId)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumber = lastInvoice.InvoiceNumber;
            if (int.TryParse(lastNumber.Replace("INV-", ""), out int parsedNumber))
            {
                nextNumber = parsedNumber + 1;
            }
        }

        return $"INV-{nextNumber:D6}";
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
    {
        // This is a placeholder implementation
        // In a real application, you would use a library like iTextSharp, PdfSharp, or similar
        // to generate actual PDF content
        
        var invoice = await GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
            throw new ArgumentException("Invoice not found");

        // For now, return a simple text representation as bytes
        var invoiceText = $@"
INVOICE
Invoice Number: {invoice.InvoiceNumber}
Date: {invoice.InvoiceDate:yyyy-MM-dd}
Due Date: {invoice.DueDate:yyyy-MM-dd}

Customer: {invoice.CustomerName}
Address: {invoice.CustomerAddress}
Phone: {invoice.CustomerPhone}

Items:
";

        foreach (var detail in invoice.Sales.SalesDetails)
        {
            invoiceText += $"{detail.Product.ProductName} - Qty: {detail.Quantity} - Price: {detail.UnitPrice:F2} - Total: {detail.TotalPrice:F2}\n";
        }

        invoiceText += $@"
Subtotal: {invoice.SubTotal:F2}
Tax: {invoice.TaxAmount:F2}
Discount: {invoice.DiscountAmount:F2}
Total: {invoice.TotalAmount:F2}

Payment Status: {invoice.PaymentStatus}
Notes: {invoice.Notes}
";

        return System.Text.Encoding.UTF8.GetBytes(invoiceText);
    }
} 