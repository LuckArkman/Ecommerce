// ECommerce.Client/Services/CartApiClient.cs
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ECommerce.Models.DTOs.Cart;

namespace ECommerce.Client.Services
{
    public class CartApiClient
    {
        private readonly HttpClient _httpClient;

        public CartApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CartItemDto>> GetCart()
        {
            return await _httpClient.GetFromJsonAsync<List<CartItemDto>>("api/cart");
        }

        public async Task<CartItemDto> AddOrUpdateCartItem(AddToCartRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/cart", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CartItemDto>();
        }

        public async Task RemoveCartItem(int productId)
        {
            var response = await _httpClient.DeleteAsync($"api/cart/{productId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task ClearCart()
        {
            var response = await _httpClient.DeleteAsync("api/cart/clear");
            response.EnsureSuccessStatusCode();
        }
    }
}