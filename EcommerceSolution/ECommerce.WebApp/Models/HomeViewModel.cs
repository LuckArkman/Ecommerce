using ECommerce.Models.DTOs.Product;
using System.Collections.Generic;

namespace ECommerce.WebApp.Models
{
    public class HomeViewModel
    {
        public List<ProductDto> newProducts { get; set; } = new List<ProductDto>();
        public List<ProductDto> mostSoldProducts { get; set; } = new List<ProductDto>();
        public List<CategoryDto> categories { get; set; } = new List<CategoryDto>();
    }
}