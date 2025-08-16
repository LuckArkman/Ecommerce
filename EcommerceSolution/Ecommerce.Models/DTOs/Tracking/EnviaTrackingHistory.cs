using Newtonsoft.Json;

namespace Ecommerce.Models.DTOs.Tracking;

public class EnviaTrackingHistory
{
    [JsonProperty("status")]
    public string? Status { get; set; } // Status do evento
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("location")]
    public string? Location { get; set; }
    [JsonProperty("timestamp")]
    public string? Timestamp { get; set; } // Data e hora do evento
}