using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Suppliers;

public class SupplierVm
{
    public int SupplierId { get; set; }
    
    [Required(ErrorMessage = "Supplier name is required")]
    [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters")]
    public string SupplierName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Contact information is required")]
    [StringLength(50, ErrorMessage = "Contact information cannot exceed 50 characters")]
    public string Contact { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string Address { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
