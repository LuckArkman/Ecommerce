using ECommerce.Models.DTOs.Order;

namespace ECommerce.Client.Services
{
    public class OrderApiClient
    {
        private readonly HttpClient _httpClient;

        public OrderApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OrderDto> CreateOrder(CreateOrderRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/orders", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderDto>();
        }

        public async Task<List<OrderDto>> GetUserOrders()
        {
            return await _httpClient.GetFromJsonAsync<List<OrderDto>>("api/orders");
        }

        public async Task<OrderDto> GetOrderById(int orderId)
        {
            return await _httpClient.GetFromJsonAsync<OrderDto>($"api/orders/{orderId}");
        }

        public async Task<List<OrderDto>> GetAllOrders()
        {
            return await _httpClient.GetFromJsonAsync<List<OrderDto>>("api/orders/all");
        }

        public async Task UpdateOrderStatus(int orderId, UpdateOrderStatusRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/orders/{orderId}/status", request);
            response.EnsureSuccessStatusCode();
        }
    }
}