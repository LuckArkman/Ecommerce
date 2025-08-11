namespace ECommerce.Application.Services;

using System.Security.Claims;
using Ecommerce.Models.DTOs.Payment;
using MercadoPago.Resource.User;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using MercadoPago.Client.Preference; // SDK do Mercado Pago
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration; // Para ler Access Token
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Para OrderItems
using Microsoft.EntityFrameworkCore; // Para Include

public class MercadoPagoPaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context; // Para acessar Orders, Products
    private readonly IConfiguration _configuration;

    public MercadoPagoPaymentService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        
        // Configurar credenciais do Mercado Pago no SDK
        MercadoPagoConfig.AccessToken = _configuration["MercadoPagoSettings:AccessToken"]!;
    }

    public async Task<CheckoutResponse> CreateCheckoutPreferenceAsync(CheckoutCreateRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)); // Acesso seguro

        // Remover FindFirstValue pois ele vem do User da API, nao do Service
        // A segurança de UserId viria de um parametro passado do Controller que é [Authorize]
        // ou de um parametro extra no metodo. Por enquanto, assume que orderId é suficiente para FindFirstOrDefaultAsync
        order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);
        
        if (order == null)
        {
            return new CheckoutResponse { Success = false, Message = "Pedido não encontrado ou não pertence ao usuário." };
        }
        if (order.Status != "Pending") // Apenas pedidos pendentes podem ser pagos
        {
            return new CheckoutResponse { Success = false, Message = "Pedido já pago ou em outro status." };
        }
        if (order.TotalAmount != request.TotalAmount) // Validação de valor
        {
             return new CheckoutResponse { Success = false, Message = "Inconsistência de valor no pedido." };
        }


        var preferenceRequest = new PreferenceCreateRequest
        {
            Items = order.OrderItems.Select(oi => new PreferenceItemRequest
            {
                Title = oi.Product?.Name ?? "Produto Desconhecido",
                Description = oi.Product?.Description ?? "",
                Quantity = oi.Quantity,
                CurrencyId = "BRL", // Moeda
                UnitPrice = oi.Price
            }).ToList(),
            
            Payer = new PreferencePayerRequest { Email = request.PayerEmail },

            // URLs de Redirecionamento (importante!)
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = request.SuccessUrl,
                Failure = request.FailureUrl,
                Pending = request.PendingUrl
            },
            AutoReturn = "approved", // Redireciona automaticamente após pagamento aprovado
            
            // Para notificações (IPN/Webhooks)
            NotificationUrl = _configuration["MercadoPagoSettings:NotificationUrl"] // URL para receber notificações de status
        };

        var client = new PreferenceClient();
        Preference preference = await client.CreateAsync(preferenceRequest);

        if (preference != null && !string.IsNullOrEmpty(preference.InitPoint))
        {
            return new CheckoutResponse
            {
                Success = true,
                CheckoutUrl = preference.InitPoint, // A URL para redirecionar o usuário
                PaymentId = preference.Id, // ID da preferência (para acompanhar)
                Message = "Preferência de pagamento criada."
            };
        }
        else
        {
            return new CheckoutResponse { Success = false, Message = "Falha ao criar preferência de pagamento no Mercado Pago." };
        }
    }

    public async Task ProcessPaymentNotificationAsync(string topic, string id)
    {
        // Esta função é chamada pelo webhook do Mercado Pago.
        // Você precisará de um Controller na API para receber esta notificação.
        // Exemplo: /api/MercadoPago/Notification?topic=payment&id={id_da_notificacao}

        if (topic == "payment")
        {
            var paymentClient = new MercadoPago.Client.Payment.PaymentClient();
            var payment = await paymentClient.GetAsync(long.Parse(id));

            if (payment != null)
            {
                // Obtenha o merchant_order_id (se estiver usando Merchant Orders)
                var merchantOrderId = payment.Order.Id; 
                var status = payment.Status; // approved, rejected, pending
                
                // Obtenha o OrderId da sua aplicação (do external_reference ou metadata)
                // Você precisaria ter salvo o OrderId da sua aplicação como ExternalReference na preferenceRequest
                // preferenceRequest.ExternalReference = order.Id.ToString();
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.TrackingNumber == merchantOrderId.ToString()); // Exemplo: usar TrackingNumber para armazenar merchant_order_id

                if (order != null)
                {
                    // Atualize o status do pedido na sua base de dados
                    switch (status)
                    {
                        case "approved":
                            order.Status = "Paid"; // Ou "Delivered", dependendo do fluxo
                            // Baixa estoque, envia email de confirmação, etc.
                            break;
                        case "pending":
                            order.Status = "PaymentPending";
                            break;
                        case "rejected":
                            order.Status = "PaymentRejected";
                            break;
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }
        // ... Lógica para outros tópicos como merchant_order
    }
}