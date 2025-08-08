namespace Ecommerce.Models.DTOs.Dashboard;

public class SalesMetricDto
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public int AverageOrderValue { get; set; }
    public Dictionary<string, decimal> SalesByMonth { get; set; } = new Dictionary<string, decimal>();
}