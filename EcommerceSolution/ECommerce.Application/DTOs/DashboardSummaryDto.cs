using ECommerce.Models.DTOs.Product;

namespace ECommerce.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public SalesMetricDto Sales { get; set; }
    public StockMetricDto Stock { get; set; }
    public DeliveryMetricDto Deliveries { get; set; }
    public CustomerSatisfactionMetricDto CustomerSatisfaction { get; set; }
    public List<ProductDto> TopRatedProducts { get; set; }
    public List<ProductDto> BestSellingProducts { get; set; }
}