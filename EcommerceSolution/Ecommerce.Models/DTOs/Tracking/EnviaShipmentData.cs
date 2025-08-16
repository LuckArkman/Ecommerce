using Newtonsoft.Json;

namespace Ecommerce.Models.DTOs.Tracking;

public class EnviaShipmentData
{
    [JsonProperty("carrier_tracking_number")]
    public string? CarrierTrackingNumber { get; set; } // O código de rastreamento
    [JsonProperty("shipment_status")]
    public string? ShipmentStatus { get; set; } // Status atual da Envia
    [JsonProperty("history")]
    public List<EnviaTrackingHistory> History { get; set; } = new List<EnviaTrackingHistory>();
}