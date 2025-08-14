namespace Ecommerce.Models.DTOs.Tracking;

public class TrackingResultDto
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty; // Status do pedido na sua base
    public bool IsDelivered { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
    public List<TrackingEventDto> Events { get; set; } = new List<TrackingEventDto>();
}