using ECommerce.Models.DTOs.Product;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductQueryParams queryParams);
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> AddProductAsync(ProductDto productDto);
        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    }
}