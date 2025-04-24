using System;
using InventoryManagementSystem.ViewModels.Sales;

namespace InventoryManagementSystem.Services.Interfaces;

public interface ISalesService
{
    Task<int> AddSaleProducts(SalesVm vm);
}
