using ECommerce.Models.DTOs.Order;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ECommerce.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderRequest request);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
        Task<OrderDto> GetOrderByIdAsync(int orderId);
        Task UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request); // Para Admin
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync(); // Para Admin
    }
}