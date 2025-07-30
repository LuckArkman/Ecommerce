using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.DTOs.User
{
    public class UpdateUserProfileRequest
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O primeiro nome não pode exceder 100 caracteres.")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "O sobrenome não pode exceder 100 caracteres.")]
        public string? LastName { get; set; }

        [StringLength(200, ErrorMessage = "O endereço não pode exceder 200 caracteres.")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "A cidade não pode exceder 100 caracteres.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "O estado não pode exceder 100 caracteres.")]
        public string? State { get; set; }

        [StringLength(20, ErrorMessage = "O CEP não pode exceder 20 caracteres.")]
        public string? ZipCode { get; set; }

        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        public string? PhoneNumber { get; set; }
    }
}