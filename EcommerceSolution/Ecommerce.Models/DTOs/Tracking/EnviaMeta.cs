using Newtonsoft.Json;

namespace Ecommerce.Models.DTOs.Tracking;

public class EnviaMeta
{
    [JsonProperty("code")]
    public int Code { get; set; }
    [JsonProperty("type")]
    public string? Type { get; set; }
    [JsonProperty("message")]
    public string? Message { get; set; }
}