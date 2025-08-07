// ECommerce.WebApp/Controllers/CartController.cs
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Claims; // Para ClaimsPrincipal, ClaimTypes
using Microsoft.AspNetCore.Authorization; // Para [Authorize]
using ECommerce.Models.DTOs.Cart; // Para CartItemDto, AddToCartRequest
using ECommerce.WebApp.Models; // Para CartViewModel
using System.Text;
using System.Linq;
using System.Net.Http.Headers; // Para AuthenticationHeaderValue

namespace ECommerce.WebApp.Controllers
{
    //[Authorize] // O carrinho deve ser vinculado a um usuário logado
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor; // Para acessar a sessão (para o JWT)

        public CartController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // Ação para exibir a página do carrinho
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new CartViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // Isso não deve acontecer com [Authorize], mas é uma checagem de segurança
                return RedirectToAction("Login", "Account"); // Redireciona se não houver userId
            }

            try
            {
                // O HttpClient "ECommerceApi" já está configurado com JwtAuthHandler no Program.cs
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                
                var apiResponse = await client.GetAsync("api/Cart"); // Chama o endpoint GetUserCart da API
                apiResponse.EnsureSuccessStatusCode();

                var cartItems = JsonConvert.DeserializeObject<List<CartItemDto>>(await apiResponse.Content.ReadAsStringAsync());
                
                viewModel.CartItems = cartItems ?? new List<CartItemDto>();
                viewModel.CartTotal = viewModel.CartItems.Sum(item => item.Subtotal);
            }
            catch (HttpRequestException ex)
            {
                // Lidar com erros da API, como 401 Unauthorized se o token expirou
                ViewBag.ErrorMessage = $"Erro ao carregar carrinho: {ex.Message}";
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Login", "Account"); // Redireciona para login em caso de auth falha
                }
            }
            catch (JsonException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao processar dados do carrinho: {ex.Message}";
            }

            return View(viewModel);
        }

        // Ação para adicionar/atualizar item via AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                // O HttpClient "ECommerceApi" já está configurado com JwtAuthHandler
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                var apiResponse = await client.PostAsJsonAsync("api/Cart", request); // Chama o endpoint AddOrUpdateCartItem
                apiResponse.EnsureSuccessStatusCode();

                var updatedCartItem = JsonConvert.DeserializeObject<CartItemDto>(await apiResponse.Content.ReadAsStringAsync());
                return Ok(updatedCartItem); // Retorna o item do carrinho atualizado
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), new { message = ex.Message });
            }
        }

        // Ação para remover item via AJAX
        [HttpDelete("{productId}")] // Mapeia para /Cart/RemoveItem/{productId} (ou /Cart/{productId} se ajustar a rota)
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCartItem(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                var apiResponse = await client.DeleteAsync($"api/Cart/{productId}"); // Chama o endpoint RemoveCartItem
                apiResponse.EnsureSuccessStatusCode();
                return NoContent(); // 204 No Content
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), new { message = ex.Message });
            }
        }

        // Ação para limpar o carrinho via AJAX
        [HttpDelete("clear")] // Mapeia para /Cart/Clear (ou /Cart/ClearCart)
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                var apiResponse = await client.DeleteAsync("api/Cart/clear"); // Chama o endpoint ClearCart
                apiResponse.EnsureSuccessStatusCode();
                return NoContent();
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), new { message = ex.Message });
            }
        }

        // Ação para obter contagem de itens do carrinho via AJAX (para o navbar)
        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Json(0); // 0 se não logado

            try
            {
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                var response = await client.GetAsync("api/Cart");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var cartItems = JsonConvert.DeserializeObject<List<CartItemDto>>(content);
                return Json(cartItems?.Sum(item => item.Quantity) ?? 0);
            }
            catch (HttpRequestException)
            {
                return Json(0); // Em caso de erro, retorna 0 (ex: API inacessível, 401)
            }
        }
    }
}