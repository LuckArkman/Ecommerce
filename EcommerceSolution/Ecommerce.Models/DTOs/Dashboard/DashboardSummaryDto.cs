using ECommerce.Models.DTOs.Product;

namespace Ecommerce.Models.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public SalesMetricDto Sales { get; set; } = new SalesMetricDto();
    public StockMetricDto Stock { get; set; } = new StockMetricDto();
    public DeliveryMetricDto Deliveries { get; set; } = new DeliveryMetricDto();
    public CustomerSatisfactionMetricDto CustomerSatisfaction { get; set; } = new CustomerSatisfactionMetricDto();
    public List<ProductDto> TopRatedProducts { get; set; } = new List<ProductDto>();
    public List<ProductDto> BestSellingProducts { get; set; } = new List<ProductDto>();
    
}