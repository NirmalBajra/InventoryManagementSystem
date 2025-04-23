using System;
using InventoryManagementSystem.ViewModels.Suppliers;

namespace InventoryManagementSystem.Services.Interfaces;

public interface ISupplierServices
{
    Task<IEnumerable<SupplierVm>> GetAllSuppliers();
    Task<SupplierVm> GetSupplierById(int id);
    Task CreateSupplier(SupplierVm vm);
    Task UpdateSupplier(SupplierVm vm);
    Task DeleteSupplier(int id);
}
