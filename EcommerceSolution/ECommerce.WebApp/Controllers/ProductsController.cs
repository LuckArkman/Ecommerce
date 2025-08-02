using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.DTOs.Product;
using ECommerce.WebApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ECommerce.Models.DTOs.Review; // Para HttpUtility

namespace ECommerce.WebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(ProductQueryParams queryParams, int pageNumber = 1)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var products = new List<ProductDto>();
            var categories = new List<CategoryDto>();
            int totalItems = 0; // Você precisará que a API retorne o total para a paginação

            try
            {
                // Chamar a API para produtos
                // Adicione lógica de paginação aqui se a API suportar (ProductQueryParams.PageNumber/PageSize)
                var apiQueryParams = new ProductQueryParams
                {
                    CategoryId = queryParams.CategoryId,
                    OrderBy = queryParams.OrderBy,
                    SearchTerm = queryParams.SearchTerm
                    // PageNumber = pageNumber,
                    // PageSize = 8 // Exemplo
                };
                var response = await client.GetAsync($"api/products?{BuildQueryString(apiQueryParams)}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                products = JsonConvert.DeserializeObject<List<ProductDto>>(content);
                totalItems = products?.Count ?? 0; // Simulação: contar todos os itens retornados pela API

                // Chamar a API para categorias
                var categoriesResponse = await client.GetAsync("api/products/categories");
                categoriesResponse.EnsureSuccessStatusCode();
                var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
                categories = JsonConvert.DeserializeObject<List<CategoryDto>>(categoriesContent);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao carregar produtos: {ex.Message}";
            }

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Categories = categories,
                QueryParams = queryParams,
                TotalItems = totalItems,
                CurrentPage = pageNumber
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            ProductDto? product = null;
            List<ProductDto>? relatedProducts = null;
            List<ReviewDto>? reviews = null;

            try
            {
                var productResponse = await client.GetAsync($"api/products/{id}");
                productResponse.EnsureSuccessStatusCode();
                product = JsonConvert.DeserializeObject<ProductDto>(await productResponse.Content.ReadAsStringAsync());

                if (product != null)
                {
                    // Produtos relacionados
                    var relatedQueryParams = new ProductQueryParams { CategoryId = product.CategoryId };
                    var relatedProductsResponse = await client.GetAsync($"api/products?{BuildQueryString(relatedQueryParams)}");
                    relatedProductsResponse.EnsureSuccessStatusCode();
                    relatedProducts = JsonConvert.DeserializeObject<List<ProductDto>>(await relatedProductsResponse.Content.ReadAsStringAsync());
                    relatedProducts = relatedProducts?.Where(p => p.Id != id).OrderBy(p => Guid.NewGuid()).Take(4).ToList();

                    // Reviews
                    var reviewsResponse = await client.GetAsync($"api/reviews/product/{id}");
                    reviewsResponse.EnsureSuccessStatusCode();
                    reviews = JsonConvert.DeserializeObject<List<ReviewDto>>(await reviewsResponse.Content.ReadAsStringAsync());
                }
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao carregar detalhes do produto: {ex.Message}";
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                Reviews = reviews
                // Calcular AverageRating, RatingCounts e TotalReviews na View Model ou Helper
            };

            return View(viewModel);
        }

        // Helper para construir a query string (duplicado, mas pode ser um método estático ou em um helper)
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