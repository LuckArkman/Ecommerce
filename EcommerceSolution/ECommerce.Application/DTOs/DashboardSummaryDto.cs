namespace ECommerce.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public SalesMetricDto Sales { get; set; }
    public StockMetricDto Stock { get; set; }
    public DeliveryMetricDto Deliveries { get; set; }
    public CustomerSatisfactionMetricDto CustomerSatisfaction { get; set; }
}