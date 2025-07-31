namespace ECommerce.Application.DTOs.Dashboard;
public class SalesMetricDto
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public int AverageOrderValue { get; set; } // Opcional, ou use decimal
    public Dictionary<string, decimal> SalesByMonth { get; set; } // "Jan": 123.45
}