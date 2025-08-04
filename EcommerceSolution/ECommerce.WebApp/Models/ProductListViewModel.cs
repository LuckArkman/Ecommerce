using ECommerce.Models.DTOs.Product;
using ECommerce.Models.DTOs.Cart;
using ECommerce.Models.DTOs.Review;
using ECommerce.Models.DTOs.User; // Se for usar para Adicionar ao Carrinho

namespace ECommerce.WebApp.Models
{
    public class ProductListViewModel
    {
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public ProductQueryParams QueryParams { get; set; } = new ProductQueryParams();
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; } = 8;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);
    }
}