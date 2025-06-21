using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagementSystem.Dtos.SalesDtos;
using InventoryManagementSystem.Entity;
using InventoryManagementSystem.ViewModels.Sales;

namespace InventoryManagementSystem.Services.Interfaces;

public interface ISalesService
{
    Task CreateSalesAsync(CreateSalesDto input, string user);
    Task<Sales> GetSalesByIdAsync(int id);
    Task<(List<Sales> sales, int totalRecords)> GetAllSalesAsync(SalesFilterDto filter);
    Task UpdateSalesAsync(int id, CreateSalesDto input, string user);
    Task DeleteSalesAsync(int id, string user);
    Task<List<Sales>> GetAllSalesAsync();
    Task<List<Sales>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
}
