using ECommerce.Models.DTOs.Product;

namespace ECommerce.Client.Services
{
    public class ProductApiClient
    {
        private readonly HttpClient _httpClient;

        public ProductApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductDto>> GetProducts()
        {
            return await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/products");
        }

        public async Task<ProductDto> GetProduct(int id)
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/{id}");
        }

        public async Task<ProductDto> CreateProduct(ProductDto product)
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task UpdateProduct(ProductDto product)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/products/{product.Id}", product);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProduct(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}