using ECommerce.Models.DTOs.Cart;

namespace ECommerce.WebApp.Models;
public class CartViewModel
{
    public List<CartItemDto> CartItems { get; set; } = new();
    public decimal CartTotal { get; set; }
}