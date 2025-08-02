using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.DTOs.Product;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace ECommerce.WebApp.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CategoryMenuViewComponent(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            List<CategoryDto> categories = new();
            try
            {
                var response = await client.GetAsync("api/products/categories");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                categories = JsonConvert.DeserializeObject<List<CategoryDto>>(content);
            }
            catch (HttpRequestException ex)
            {
                // Logar o erro, mas não impedir a renderização do resto da página
                Console.WriteLine($"Erro ao carregar categorias para o menu: {ex.Message}");
            }
            return View(categories);
        }
    }
}