using ECommerce.Models.DTOs.Order;
using System.Collections.Generic;
namespace ECommerce.WebApp.Models
{
    public class PendingOrdersViewModel
    {
        public List<OrderDto>? Orders { get; set; }
    }
}