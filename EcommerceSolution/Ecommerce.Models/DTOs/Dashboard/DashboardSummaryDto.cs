using ECommerce.Models.DTOs.Product; // para LowStockProducts
using ECommerce.Models.DTOs.Review;  // para RecentReviews

namespace ECommerce.Models.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public SalesMetricDto Sales { get; set; } = new SalesMetricDto();
        public StockMetricDto Stock { get; set; } = new StockMetricDto();
        public DeliveryMetricDto Deliveries { get; set; } = new DeliveryMetricDto();
        public CustomerSatisfactionMetricDto CustomerSatisfaction { get; set; } = new CustomerSatisfactionMetricDto();
    }
}