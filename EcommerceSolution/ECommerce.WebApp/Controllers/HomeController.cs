using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web; // Para HttpUtility
using System.Collections.Generic;
using System.Linq;
using ECommerce.Models.DTOs.Product;
using ECommerce.WebApp.Models;

namespace ECommerce.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new HomeViewModel(); // Instancia o ViewModel aqui

            try
            {
                // 1. Chamar a API para "Lançamentos" (produtos mais novos)
                var newQueryParams = new ProductQueryParams { OrderBy = "newest" };
                var newProductsResponse = await client.GetAsync($"api/products?{BuildQueryString(newQueryParams)}");
                newProductsResponse.EnsureSuccessStatusCode();
                var newProductsContent = await newProductsResponse.Content.ReadAsStringAsync();
                viewModel.newProducts = JsonConvert.DeserializeObject<List<ProductDto>>(newProductsContent)?.Take(8).ToList() ?? new List<ProductDto>();

                // 2. Chamar a API para "Mais Vendidos" (simulação ou real, se API suportar)
                var mostSoldQueryParams = new ProductQueryParams { OrderBy = "priceDesc" }; // Exemplo: mais caros, ou aleatório
                var mostSoldProductsResponse = await client.GetAsync($"api/products?{BuildQueryString(mostSoldQueryParams)}");
                mostSoldProductsResponse.EnsureSuccessStatusCode();
                var mostSoldProductsContent = await mostSoldProductsResponse.Content.ReadAsStringAsync();
                viewModel.mostSoldProducts = JsonConvert.DeserializeObject<List<ProductDto>>(mostSoldProductsContent)?.OrderBy(p => Guid.NewGuid()).Take(8).ToList() ?? new List<ProductDto>();


                // 3. Chamar a API para Categorias
                var categoriesResponse = await client.GetAsync("api/products/categories");
                categoriesResponse.EnsureSuccessStatusCode();
                var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
                viewModel.categories = JsonConvert.DeserializeObject<List<CategoryDto>>(categoriesContent) ?? new List<CategoryDto>();
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao carregar dados da homepage: {ex.Message}";
                // Em produção, você pode ter uma página de erro ou logar isso.
            }
            catch (JsonException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao deserializar dados da homepage: {ex.Message}";
            }

            // Remove as linhas de ViewBag, pois agora estamos usando o ViewModel
            // ViewBag.NewProducts = newProducts;
            // ViewBag.MostSoldProducts = mostSoldProducts;
            // ViewBag.Categories = categories;

            return View(viewModel); // Passe o viewModel para a View
        }
        
        private string BuildQueryString(ProductQueryParams queryParams)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (queryParams.CategoryId.HasValue && queryParams.CategoryId.Value > 0)
            {
                query["CategoryId"] = queryParams.CategoryId.Value.ToString();
            }
            if (!string.IsNullOrWhiteSpace(queryParams.OrderBy))
            {
                query["OrderBy"] = queryParams.OrderBy;
            }
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                query["SearchTerm"] = queryParams.SearchTerm;
            }
            return query.ToString();
        }
    }
}