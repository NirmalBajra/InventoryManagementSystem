using System.Collections.Generic;
using InventoryManagementSystem.Dtos.SalesDtos;
using InventoryManagementSystem.ViewModels.Product;

namespace InventoryManagementSystem.ViewModels.Sales
{
    public class SalesEditVm
    {
        public CreateSalesDto Sale { get; set; }
        public List<ProductVm> Products { get; set; }
    }
} 