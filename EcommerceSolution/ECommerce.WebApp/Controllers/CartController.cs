using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using ECommerce.Models.DTOs.Cart;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization; // Para o [Authorize]
using ECommerce.Models.DTOs.User;
using ECommerce.WebApp.Models;

namespace ECommerce.WebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor; // Para acessar cookies/sessão se precisar

        public CartController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // Ação para adicionar/atualizar item via AJAX
        [HttpPost]
        [Authorize] // Exige que o usuário esteja logado
        [ValidateAntiForgeryToken] // Proteção CSRF
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtém o ID do usuário logado no MVC Identity

            // Repassa o token JWT para a API, se estiver usando autenticação JWT
            var token = HttpContext.Session.GetString("JwtToken"); // Ou de um cookie seguro
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            try
            {
                // Chamar a API de backend para adicionar ao carrinho
                var response = await client.PostAsJsonAsync("api/cart", request);
                response.EnsureSuccessStatusCode();
                return Ok();
            }
            catch (HttpRequestException ex)
            {
                // Captura a resposta de erro da API
                string? errorContent = ex.StatusCode.ToString();
                return StatusCode(((int)ex.StatusCode.Value), new { message = errorContent ?? "Erro ao adicionar ao carrinho na API." });
            }
        }

        // Ação para obter contagem de itens do carrinho via AJAX (para o navbar)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCartItemCount()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Repassa o token JWT
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Json(0); // Retorna 0 se não houver usuário logado
            }

            try
            {
                var response = await client.GetAsync("api/cart");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var cartItems = JsonConvert.DeserializeObject<List<CartItemDto>>(content);
                return Json(cartItems?.Sum(item => item.Quantity) ?? 0);
            }
            catch (HttpRequestException)
            {
                return Json(0); // Em caso de erro, retorna 0
            }
        }

        // Action para exibir o carrinho
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cartItems = new List<CartItemDto>();
            try
            {
                var response = await client.GetAsync("api/cart");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                cartItems = JsonConvert.DeserializeObject<List<CartItemDto>>(content);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao carregar carrinho: {ex.Message}";
            }

            var viewModel = new CartViewModel
            {
                CartItems = cartItems,
                CartTotal = cartItems.Sum(item => item.Subtotal)
            };

            return View(viewModel);
        }

        // Implemente UpdateQuantity e RemoveItem no CartController
        // Eles também receberiam requisições AJAX e chamariam a API de backend.
    }
}