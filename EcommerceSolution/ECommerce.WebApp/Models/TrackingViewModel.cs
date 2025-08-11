using System.ComponentModel.DataAnnotations;
using Ecommerce.Models.DTOs.Tracking;

namespace ECommerce.WebApp.Models
{
    public class TrackingViewModel
    {
        [Display(Name = "Código de Rastreamento")]
        [Required(ErrorMessage = "O código de rastreamento é obrigatório.")]
        public string? TrackingNumberInput { get; set; }

        public TrackingResultDto? TrackingResult { get; set; }
    }
}