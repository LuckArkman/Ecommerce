using ECommerce.Models.DTOs.Product;

namespace ECommerce.Models.DTOs.Dashboard;

public class StockMetricDto
{
    public int TotalProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public List<ProductDto> LowStockProducts { get; set; } = new List<ProductDto>();
}