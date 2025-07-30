using ECommerce.Models.DTOs.Cart;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.DTOs.Order
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "O endereço de entrega é obrigatório.")]
        [StringLength(500, ErrorMessage = "O endereço não pode exceder 500 caracteres.")]
        public string ShippingAddress { get; set; } = string.Empty;
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }
}