using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.DTOs.User
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        public string password { get; set; } = string.Empty;
    }
}