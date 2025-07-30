using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.DTOs.Review
{
    public class CreateReviewRequest
    {
        [Required]
        public int ProductId { get; set; }
        [Range(1, 5, ErrorMessage = "A avaliação deve ser entre 1 e 5.")]
        public int Rating { get; set; }
        [StringLength(500, ErrorMessage = "O comentário não pode exceder 500 caracteres.")]
        public string? Comment { get; set; }
    }
}