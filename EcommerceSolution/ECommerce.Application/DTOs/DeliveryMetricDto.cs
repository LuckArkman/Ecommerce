namespace ECommerce.Application.DTOs.Dashboard;

public class DeliveryMetricDto
{
    public int PendingDeliveries { get; set; }
    public int ShippedDeliveries { get; set; }
    public int DeliveredDeliveries { get; set; }
    public int CancelledDeliveries { get; set; }
}