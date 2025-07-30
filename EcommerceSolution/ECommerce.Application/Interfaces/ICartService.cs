using ECommerce.Models.DTOs.Cart;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ECommerce.Application.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemDto>> GetUserCartAsync(string userId);
        Task<CartItemDto> AddOrUpdateCartItemAsync(string userId, AddToCartRequest request);
        Task RemoveCartItemAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
    }
}