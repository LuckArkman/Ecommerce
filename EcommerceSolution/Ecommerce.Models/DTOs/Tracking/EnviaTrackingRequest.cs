using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs.Tracking;

public class EnviaTrackingRequest
{
    [Required]
    public List<string> tracking_numbers { get; set; } = new List<string>();
}