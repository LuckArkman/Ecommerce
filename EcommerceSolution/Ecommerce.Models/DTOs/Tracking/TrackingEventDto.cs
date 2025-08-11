namespace Ecommerce.Models.DTOs.Tracking;

public class TrackingEventDto
{
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DateTime { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}