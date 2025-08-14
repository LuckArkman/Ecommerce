using Ecommerce.Models.DTOs.Payment;
using MercadoPago.NetCore.Model.Resources.Enum;

namespace ECommerce.Application.Services;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using Newtonsoft.Json;

public class MercadoPagoPaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient; // <--- INJETAR HTTPCLIENT AQUI

    public MercadoPagoPaymentService(ApplicationDbContext context, IConfiguration configuration, HttpClient httpClient) // <--- ADICIONAR AO CONSTRUTOR
    {
        _context = context;
        _configuration = configuration;
        _httpClient = httpClient; // <--- ATRIBUIR AQUI
        
        MercadoPagoConfig.AccessToken = _configuration["MercadoPagoSettings:AccessToken"]!;

        // Definir BaseAddress para chamadas de Payment API se for diferente
        // API de Pagamentos do Mercado Pago: https://api.mercadopago.com/v1/payments/{id}
        // A API principal do MP é: https://api.mercadopago.com/
        // O HttpClient injetado pode já ter uma base URL, se não, pode ser definida aqui
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri("https://api.mercadopago.com/");
        }
    }

    public async Task<CheckoutResponse> CreateCheckoutPreferenceAsync(CheckoutCreateRequest request)
    {
        // ... (código existente para CreateCheckoutPreferenceAsync) ...
        // Este método já usa o SDK PreferenceClient, que deve estar funcionando
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId); 
        
        if (order == null) return new CheckoutResponse { Success = false, Message = "Pedido não encontrado." };
        if (order.Status != "Pending") return new CheckoutResponse { Success = false, Message = "Pedido já pago ou em outro status." };
        if (order.TotalAmount != request.TotalAmount) return new CheckoutResponse { Success = false, Message = "Inconsistência de valor no pedido." };

        var preferenceRequest = new MercadoPago.Client.Preference.PreferenceRequest
        {
            Items = order.OrderItems.Select(oi => new PreferenceItemRequest
            {
                Title = oi.Product?.Name ?? "Produto Desconhecido",
                Description = oi.Product?.Description ?? "",
                Quantity = oi.Quantity,
                CurrencyId = CurrencyId.BRL.ToString(),
                UnitPrice = oi.Price
            }).ToList(),
            Payer = new PreferencePayerRequest { Email = request.PayerEmail },
            BackUrls = new PreferenceBackUrlsRequest 
            { 
                Success = request.SuccessUrl, 
                Failure = request.FailureUrl, 
                Pending = request.PendingUrl 
            },
            AutoReturn = "approved",
            NotificationUrl = _configuration["MercadoPagoSettings:NotificationUrl"],
            ExternalReference = order.Id.ToString() // Recommended: Link to order
        };

        var client = new PreferenceClient();
        Preference preference = await client.CreateAsync(preferenceRequest);

        if (preference != null && !string.IsNullOrEmpty(preference.InitPoint))
        {
            return new CheckoutResponse { Success = true, CheckoutUrl = preference.InitPoint, PaymentId = preference.Id, Message = "Preferência de pagamento criada." };
        }
        else
        {
            return new CheckoutResponse { Success = false, Message = "Falha ao criar preferência de pagamento no Mercado Pago." };
        }
    }

    // ***** IMPLEMENTAÇÃO DE ProcessPaymentNotificationAsync COM HTTPCLIENT MANUAL *****
    public async Task ProcessPaymentNotificationAsync(string topic, string id)
    {
        // A notificação de pagamento (topic = "payment") envia o ID do pagamento.
        // Precisamos chamar a API do Mercado Pago para obter detalhes desse pagamento.

        if (topic == "payment")
        {
            // Adicionar cabeçalho de autorização para a chamada da API de Pagamentos
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", MercadoPagoConfig.AccessToken);

            // Endpoint da API de Pagamentos: https://api.mercadopago.com/v1/payments/{id}
            var requestUrl = $"v1/payments/{id}";

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode(); // Lança exceção para 4xx/5xx

                var content = await response.Content.ReadAsStringAsync();
                // DTO simplificado para a resposta da API de Pagamento (adapte conforme a resposta real)
                var paymentDetails = JsonConvert.DeserializeObject<MercadoPagoPaymentDetails>(content); 

                if (paymentDetails != null)
                {
                    // ID da Merchant Order se você estiver usando Merchant Orders
                    var merchantOrderId = paymentDetails.Order?.Id; 
                    var status = paymentDetails.Status; // approved, rejected, pending

                    // Você precisaria ter salvo o OrderId da sua aplicação como ExternalReference na preferenceRequest
                    // Ex: preferenceRequest.ExternalReference = order.Id.ToString();
                    // Então, buscaria assim: order = await _context.Orders.FirstOrDefaultAsync(o => o.Id.ToString() == paymentDetails.ExternalReference);
                    
                    // Como estamos usando TrackingNumber para armazenar merchant_order_id no exemplo:
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.TrackingNumber == merchantOrderId.ToString());

                    if (order != null)
                    {
                        switch (status)
                        {
                            case "approved":
                                order.Status = "Paid";
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
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro HTTP ao buscar detalhes do pagamento {id}: {ex.Message}");
                // Logar o erro, talvez notificar um sistema de monitoramento
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erro JSON ao buscar detalhes do pagamento {id}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado ao processar notificação {id}: {ex.Message}");
            }
        }
    }
}