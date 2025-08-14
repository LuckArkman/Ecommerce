// ECommerce.WebApp/Controllers/CheckoutController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ECommerce.Models.DTOs.Cart; // Para CartItemDto
using ECommerce.Models.DTOs.Order; // Para OrderDto, CreateOrderRequest
using ECommerce.WebApp.Models; // Para CartViewModel, CheckoutViewModel
using System.Linq;
using Ecommerce.Models.DTOs.Payment;

namespace ECommerce.WebApp.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CheckoutController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Exibir a página de checkout (revisão do pedido, endereço)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            CartViewModel cartViewModel = new();

            try
            {
                var cartResponse = await client.GetAsync("api/Cart");
                cartResponse.EnsureSuccessStatusCode();
                var cartItems = JsonConvert.DeserializeObject<List<CartItemDto>>(await cartResponse.Content.ReadAsStringAsync());

                cartViewModel.CartItems = cartItems ?? new List<CartItemDto>();
                cartViewModel.CartTotal = cartViewModel.CartItems.Sum(item => item.Subtotal);

                if (!cartViewModel.CartItems.Any())
                {
                    TempData["ErrorMessage"] = "Seu carrinho está vazio. Adicione itens antes de finalizar a compra.";
                    return RedirectToAction("Index", "Cart"); // Redireciona para o carrinho vazio
                }
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao carregar carrinho para checkout: {ex.Message}";
                return View("Error"); // Página de erro genérica
            }

            var viewModel = new CheckoutViewModel { Cart = cartViewModel, ShippingAddress = "Rua Exemplo, 123 - Cidade, Estado, CEP" }; // Preencher com endereço do usuário
            return View(viewModel);
        }

        // Processar o pedido e iniciar o pagamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                // Se a validação falhar, recarregar o carrinho e retornar a View
                client = _httpClientFactory.CreateClient("ECommerceApi");
                var cartResponse = await client.GetAsync("api/Cart");
                cartResponse.EnsureSuccessStatusCode();
                viewModel.Cart.CartItems = JsonConvert.DeserializeObject<List<CartItemDto>>(await cartResponse.Content.ReadAsStringAsync());
                viewModel.Cart.CartTotal = viewModel.Cart.CartItems.Sum(item => item.Subtotal);
                return View("Index", viewModel);
            }

            client = _httpClientFactory.CreateClient("ECommerceApi");
            try
            {
                // 1. Criar o pedido na sua API
                var createOrderRequest = new CreateOrderRequest
                {
                    ShippingAddress = viewModel.ShippingAddress,
                    CartItems = viewModel.Cart.CartItems.ToList() // Certifique-se que CartItems não é nulo
                };
                var orderResponse = await client.PostAsJsonAsync("api/Orders", createOrderRequest);
                orderResponse.EnsureSuccessStatusCode();
                var order = JsonConvert.DeserializeObject<OrderDto>(await orderResponse.Content.ReadAsStringAsync());

                if (order == null || order.Id == 0)
                {
                    TempData["ErrorMessage"] = "Erro ao criar o pedido.";
                    return RedirectToAction("Index", "Cart");
                }

                // 2. Criar a Preferência de Pagamento no Mercado Pago via sua API de Backend
                var checkoutRequest = new CheckoutCreateRequest
                {
                    OrderId = order.Id,
                    OrderDescription = $"Pedido #{order.Id} de {User.Identity.Name}",
                    TotalAmount = order.TotalAmount,
                    PayerEmail = User.Identity.Name, // Use o email do usuário logado
                    SuccessUrl = Url.Action("PaymentSuccess", "Checkout", new { orderId = order.Id }, Request.Scheme),
                    FailureUrl = Url.Action("PaymentFailure", "Checkout", new { orderId = order.Id }, Request.Scheme),
                    PendingUrl = Url.Action("PaymentPending", "Checkout", new { orderId = order.Id }, Request.Scheme)
                };

                var mpResponse = await client.PostAsJsonAsync("api/Dashboard/mercadopago/create-preference", checkoutRequest); // NOVO ENDPOINT NA API
                mpResponse.EnsureSuccessStatusCode();
                var checkoutResult = JsonConvert.DeserializeObject<CheckoutResponse>(await mpResponse.Content.ReadAsStringAsync());

                if (checkoutResult != null && checkoutResult.Success && !string.IsNullOrEmpty(checkoutResult.CheckoutUrl))
                {
                    // Redirecionar o usuário para o Checkout Pro do Mercado Pago
                    return Redirect(checkoutResult.CheckoutUrl);
                }
                else
                {
                    TempData["ErrorMessage"] = checkoutResult?.Message ?? "Erro ao iniciar pagamento com Mercado Pago.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro na comunicação com a API: {ex.Message}";
                return View("Error");
            }
            catch (JsonException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao processar dados: {ex.Message}";
                return View("Error");
            }
        }

        // Páginas de retorno do Mercado Pago
        [HttpGet]
        public IActionResult PaymentSuccess(int orderId)
        {
            ViewBag.Message = $"Seu pagamento para o pedido #{orderId} foi aprovado! Agradecemos a sua compra.";
            // Atualizar status do pedido no DB (já deve ser feito pelo webhook)
            return View("PaymentStatus");
        }

        [HttpGet]
        public IActionResult PaymentPending(int orderId)
        {
            ViewBag.Message = $"Seu pagamento para o pedido #{orderId} está pendente. Assim que for aprovado, você receberá a confirmação.";
            return View("PaymentStatus");
        }

        [HttpGet]
        public IActionResult PaymentFailure(int orderId)
        {
            ViewBag.Message = $"Seu pagamento para o pedido #{orderId} falhou. Por favor, tente novamente.";
            return View("PaymentStatus");
        }
    }
}