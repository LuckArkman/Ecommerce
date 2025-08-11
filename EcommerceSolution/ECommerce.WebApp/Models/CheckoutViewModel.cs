// ECommerce.WebApp/Models/CheckoutViewModel.cs
using ECommerce.Models.DTOs.Order; // Para OrderDto
using ECommerce.Models.DTOs.Cart; // Para CartItemDto
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ECommerce.WebApp.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "O endereço de entrega é obrigatório.")]
        [StringLength(500, ErrorMessage = "O endereço não pode exceder 500 caracteres.")]
        public string ShippingAddress { get; set; } = string.Empty;

        public CartViewModel Cart { get; set; } = new CartViewModel();
    }
}