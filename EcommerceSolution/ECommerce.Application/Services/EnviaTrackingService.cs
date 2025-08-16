using System.Net.Http.Headers;
using System.Text;
using ECommerce.Application.Interfaces;
using Ecommerce.Models.DTOs.Tracking;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace ECommerce.Application.Services;

public class EnviaTrackingService : ITrackingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;

    public EnviaTrackingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiKey = _configuration["EnviaApi:ApiKey"]!;
        
        // Configurar a Base URL da API da Envia.com
        _httpClient.BaseAddress = new Uri(_configuration["EnviaApi:BaseUrl"] ?? "https://api.envia.com");
        // Adicionar cabeçalho de autorização padrão para todas as requisições deste cliente
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<TrackingResultDto> TrackOrderAsync(string trackingNumber)
    {
        var result = new TrackingResultDto { TrackingNumber = trackingNumber };
        var requestBody = new EnviaTrackingRequest { tracking_numbers = new List<string> { trackingNumber } };
        
        var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        try
        {
            // Endpoint POST /track para rastreamento
            var response = await _httpClient.PostAsync("track", jsonContent);
            response.EnsureSuccessStatusCode(); // Lança exceção para 4xx/5xx

            var content = await response.Content.ReadAsStringAsync();
            var enviaResponse = JsonConvert.DeserializeObject<EnviaTrackingResponse>(content);

            if (enviaResponse?.Data?.Any() == true)
            {
                var shipmentData = enviaResponse.Data.FirstOrDefault();
                if (shipmentData != null)
                {
                    result.OrderStatus = shipmentData.ShipmentStatus ?? "Status Desconhecido";
                    result.IsDelivered = shipmentData.ShipmentStatus?.Equals("DELIVERED", StringComparison.OrdinalIgnoreCase) ?? false;

                    // Mapear eventos
                    result.Events = shipmentData.History
                        .OrderByDescending(h => DateTime.Parse(h.Timestamp ?? DateTime.MinValue.ToString())) // Ordenar por data/hora
                        .Select(h => new TrackingEventDto
                        {
                            Status = h.Status ?? "N/A",
                            Description = h.Description ?? "Sem descrição",
                            DateTime = DateTime.Parse(h.Timestamp ?? DateTime.MinValue.ToString()).ToString("dd/MM/yyyy HH:mm:ss"), // Formatar data
                            Location = h.Location ?? "N/A"
                        })
                        .ToList();
                }
            }
            else
            {
                result.IsError = true;
                result.ErrorMessage = enviaResponse?.Meta?.Message ?? "Código de rastreamento não encontrado ou sem eventos na Envia.com.";
            }
        }
        catch (HttpRequestException ex)
        {
            result.IsError = true;
            result.ErrorMessage = $"Erro de comunicação com a API da Envia.com: {ex.Message}";
            // Se 401, token inválido; se 404, endpoint; se 422, validação de input
        }
        catch (JsonException ex)
        {
            result.IsError = true;
            result.ErrorMessage = $"Erro ao processar resposta da Envia.com: {ex.Message}";
        }
        catch (Exception ex)
        {
            result.IsError = true;
            result.ErrorMessage = $"Ocorreu um erro inesperado ao rastrear: {ex.Message}";
        }

        return result;
    }
}