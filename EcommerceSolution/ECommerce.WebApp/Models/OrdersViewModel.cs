using ECommerce.Models.DTOs.Order;
using System.Collections.Generic;
namespace ECommerce.WebApp.Models
{
    public class OrdersViewModel
    {
        public List<OrderDto>? Orders { get; set; }
    }
}