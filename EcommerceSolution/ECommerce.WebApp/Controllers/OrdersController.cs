// ECommerce.WebApp/Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Ecommerce.Models.DTOs.Tracking;
using ECommerce.WebApp.Models;

namespace ECommerce.WebApp.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ... (Actions Index, PlaceOrder, PaymentSuccess, PaymentPending, PaymentFailure) ...

        // Action para exibir a página de rastreamento
        [HttpGet]
        public async Task<IActionResult> Track(string trackingNumber)
        {
            // O HttpClient "ECommerceApi" já tem o JwtAuthHandler
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new TrackingViewModel(); // Crie este ViewModel

            if (string.IsNullOrEmpty(trackingNumber))
            {
                // Se o código de rastreamento não foi fornecido, o usuário pode digitar
                return View(viewModel); // Exibe a view vazia para o usuário digitar
            }

            try
            {
                // Chamada para o endpoint de rastreamento na sua API
                var apiResponse = await client.GetAsync($"api/Tracking/{trackingNumber}");
                apiResponse.EnsureSuccessStatusCode();

                var trackingResult = JsonConvert.DeserializeObject<TrackingResultDto>(await apiResponse.Content.ReadAsStringAsync());
                viewModel.TrackingResult = trackingResult;

                if (trackingResult?.IsError == true)
                {
                    ViewBag.ErrorMessage = trackingResult.ErrorMessage;
                }
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao rastrear pedido: {ex.Message}";
            }
            catch (JsonException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao processar dados de rastreamento: {ex.Message}";
            }

            return View(viewModel); // Retorna Views/Orders/Track.cshtml
        }
    }
}