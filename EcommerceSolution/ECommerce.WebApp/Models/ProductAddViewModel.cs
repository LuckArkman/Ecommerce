using ECommerce.Models.DTOs.Product;
using System.Collections.Generic;
namespace ECommerce.WebApp.Models
{
    public class ProductAddViewModel
    {
        public ProductDto Product { get; set; } = new ProductDto();
        public List<CategoryDto>? Categories { get; set; }
    }
}