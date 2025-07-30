using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.DTOs.Order
{
    public class UpdateOrderStatusRequest
    {
        [Required(ErrorMessage = "O status é obrigatório.")]
        public string Status { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
    }
}