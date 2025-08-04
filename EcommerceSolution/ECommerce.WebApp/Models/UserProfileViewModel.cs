// ECommerce.WebApp/Models/UserProfileViewModel.cs
using ECommerce.Models.DTOs.User; // Para UserProfileDto, UpdateUserProfileRequest
using System.ComponentModel.DataAnnotations; // Para validação

namespace ECommerce.WebApp.Models
{
    public class UserProfileViewModel
    {
        public UserProfileDto UserProfile { get; set; } = new UserProfileDto(); // Dados para exibição
        public UpdateUserProfileRequest UpdateRequest { get; set; } = new UpdateUserProfileRequest(); // Dados para edição

        public bool IsEditMode { get; set; } = false; // Controla se o formulário está em modo de edição
        public string? Message { get; set; } // Mensagem de sucesso/erro
        public bool IsSuccess { get; set; } // Indicador de sucesso da mensagem
    }
}