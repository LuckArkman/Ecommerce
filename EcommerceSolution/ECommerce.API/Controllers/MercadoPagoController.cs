using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MercadoPagoController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<MercadoPagoController> _logger;

        public MercadoPagoController(IPaymentService paymentService, ILogger<MercadoPagoController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // Endpoint para receber notificações do Mercado Pago (IPN/Webhooks)
        // Ex: POST /api/MercadoPago/Notification?topic={topic}&id={id}
        // O Mercado Pago envia GET e POST, precisamos lidar com ambos se necessário.
        // O corpo da notificação pode ser vazio ou conter JSON, dependendo do tópico.
        [HttpPost("Notification")]
        [HttpGet("Notification")] // O Mercado Pago pode enviar GET para validação
        public async Task<IActionResult> HandleNotification([FromQuery] string topic, [FromQuery] string id)
        {
            _logger.LogInformation($"Notificação Mercado Pago recebida. Tópico: {topic}, ID: {id}");

            if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(id))
            {
                return BadRequest("Parâmetros 'topic' ou 'id' ausentes.");
            }

            try
            {
                await _paymentService.ProcessPaymentNotificationAsync(topic, id);
                return Ok(); // Resposta 200 OK é crucial para o Mercado Pago parar de enviar a notificação
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar notificação do Mercado Pago (Tópico: {topic}, ID: {id}).");
                return StatusCode(500, $"Erro interno ao processar notificação: {ex.Message}");
            }
        }
    }
}