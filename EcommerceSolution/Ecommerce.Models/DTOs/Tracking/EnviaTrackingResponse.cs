using Newtonsoft.Json;

namespace Ecommerce.Models.DTOs.Tracking;

public class EnviaTrackingResponse
{
    [JsonProperty("meta")]
    public EnviaMeta? Meta { get; set; }

    [JsonProperty("data")]
    public List<EnviaShipmentData> Data { get; set; } = new List<EnviaShipmentData>();
}